using ACUEDUCTO.Components;
using ACUEDUCTO.Models;
using ACUEDUCTO.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ACUEDUCTO.API.Options;
using ACUEDUCTO.API.Services;

namespace ACUEDUCTO;

public class Program
{
    public static void Main(string[] args)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddHttpClient();

        // Option B: do not expose /api/* endpoints. Use server-side services directly.
        builder.Services.AddQuickGridEntityFrameworkAdapter();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<EmailService>(sp =>
            new EmailService(
                smtpServer: builder.Configuration["EmailSettings:SmtpServer"],
                smtpPort: int.Parse(builder.Configuration["EmailSettings:SmtpPort"] ?? "25"),
                smtpUser: builder.Configuration["EmailSettings:SmtpUser"],
                smtpPassword: builder.Configuration["EmailSettings:SmtpPassword"]
            ));

        // Registrar EmailTokenService (faltaba) usando secreto desde configuración
        var tokenSecret = builder.Configuration["EmailTokenSecret"] ?? "dev-secret-change";
        builder.Services.AddSingleton<EmailTokenService>(new EmailTokenService(tokenSecret));

        builder.Services.AddScoped<IMenuPermissionService, MenuPermissionService>();
        builder.Services.AddScoped<IPdfService, PdfService>();
        builder.Services.AddScoped<PdfProcessorService>();
        builder.Services.AddScoped<FileDeletionService>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContextFactory<AcueductoContext>(options => options.UseSqlServer(connectionString));

        builder.Services.AddOptions<OcrOptions>()
            .Bind(builder.Configuration.GetSection(OcrOptions.SectionName))
            .ValidateDataAnnotations();

        builder.Services.AddHttpClient<GptVisionConsumoOcrService>();

        builder.Services.AddScoped<IConsumoOcrService>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OcrOptions>>().Value;
            if (opts.UseGptVisionAlways
                && !string.IsNullOrWhiteSpace(opts.GptVisionEndpoint)
                && !string.IsNullOrWhiteSpace(opts.GptVisionApiKey))
                return sp.GetRequiredService<GptVisionConsumoOcrService>();

            return ActivatorUtilities.CreateInstance<TesseractConsumoOcrService>(sp);
        });
        builder.Services.AddScoped<ConsumoOcrWorkflow>();
        builder.Services.AddScoped<ConsumoOcrAppService>();
        builder.Services.Configure<WhatsAppOptions>(builder.Configuration.GetSection("WhatsApp"));
        builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>(c => c.BaseAddress = new Uri("https://graph.facebook.com/"));

        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

        var app = builder.Build();

        if (app.Environment.IsDevelopment()) app.UseWebAssemblyDebugging();
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
            app.UseMigrationsEndPoint();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.UseAuthorization();

        // API endpoint for client-side PDF save
        app.MapPost("/api/pdf/save", async (PdfSaveRequest req, IWebHostEnvironment env) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Base64) || string.IsNullOrWhiteSpace(req.FileName))
                    return Results.BadRequest(new { error = "Base64 y FileName son requeridos" });

                var safeUser = Sanitize(req.CodigoUsuario ?? "usuario");
                var safeFile = Sanitize(req.FileName);
                
                var userDir = Path.Combine(env.WebRootPath, "imagenes", safeUser);
                Directory.CreateDirectory(userDir);
                
                var filePath = Path.Combine(userDir, safeFile);
                var pdfBytes = Convert.FromBase64String(req.Base64);
                await File.WriteAllBytesAsync(filePath, pdfBytes);
                
                var relativePath = $"/imagenes/{safeUser}/{safeFile}";
                return Results.Ok(new { path = relativePath });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error guardando PDF: {ex.Message}");
            }
        }).DisableAntiforgery();

        // API endpoint to borrar facturacions por idplanilla (used by client-side JS fallback)
        app.MapPost("/api/planillas/{idplanilla}/borrar-facturacions", async (string idplanilla, IDbContextFactory<AcueductoContext> dbFactory) =>
        {
            try
            {
                await using var ctx = dbFactory.CreateDbContext();
                var list = await ctx.Facturacions.Where(f => f.Idplanilla == idplanilla).ToListAsync();
                if (list == null || !list.Any())
                    return Results.Ok(new { deleted = 0 });

                ctx.Facturacions.RemoveRange(list);

                var plan = await ctx.Planillas.FirstOrDefaultAsync(p => p.Idplanilla == idplanilla);
                if (plan != null)
                {
                    plan.TotalPredios = 0;
                    plan.CantidadConsumoM3total = 0;
                    plan.ValorTotalConsumoM3 = 0;
                    plan.ValorTotalMora = 0;
                    plan.ValorTotalSobreTasa = 0;
                    plan.ValorTotalSubsidio = 0;
                    plan.ValorTotalOtros = 0;
                    plan.ValorTotalPlanilla = 0;
                    plan.ValorTotalRecaudo = 0;
                }

                await ctx.SaveChangesAsync();

                return Results.Ok(new { deleted = list.Count });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).DisableAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();

        static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", input.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        static bool VerifySha256(string password, byte[] salt, byte[] expected)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);
            var data = new byte[salt.Length + pwdBytes.Length];
            Buffer.BlockCopy(salt,0,data,0,salt.Length);
            Buffer.BlockCopy(pwdBytes,0,data,salt.Length,pwdBytes.Length);
            var hash = sha.ComputeHash(data);
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(hash, expected);
        }
    }
}

public sealed class PdfSaveRequest
{
    public string? Base64 { get; set; }
    public string? FileName { get; set; }
    public string? CodigoUsuario { get; set; }
}

