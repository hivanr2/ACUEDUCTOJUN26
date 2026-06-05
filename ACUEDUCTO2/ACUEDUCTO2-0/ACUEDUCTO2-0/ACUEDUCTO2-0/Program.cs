
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ACUEDUCTO2-0.Components;
using ACUEDUCTO2.Components.Account;
using ACUEDUCTO2-0.Data;
using ACUEDUCTO2-0.Share.Models;
using ACUEDUCTO2-0.Compartir.MODELS;
using ACUEDUCTO2-0.Components;
using ACUEDUCTO2-0.Data;
using Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Persist DataProtection keys to disk so external auth correlation cookies and
// protected payloads survive application restarts and multiple instances.
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .SetApplicationName("ACUEDUCTO2-0")
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        // Ensure the external provider redirects to the HTTP endpoint we added
        options.CallbackPath = "/Account/ExternalLoginCallback";
        // Make the correlation cookie usable after redirect from external provider
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
        // Ensure the external provider redirects to the HTTP endpoint we added
        options.CallbackPath = "/Account/ExternalLoginCallback";
        // Make the correlation cookie usable after redirect from external provider
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddIdentityCookies();

// Ensure cookie policy permits SameSite=None for cross-site auth redirects
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register the factory (singleton) and avoid AddDbContext to prevent lifetime conflict
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Expose a scoped ApplicationDbContext created from the factory so Identity and scoped consumers keep working
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    return factory.CreateDbContext();
});

// Register factory for the source database (xcursoro_acueducto) so we can read/copy data
var xcursoroConnection = builder.Configuration.GetConnectionString("XcursoroConnection");
if (!string.IsNullOrEmpty(xcursoroConnection))
{
    builder.Services.AddDbContextFactory<XcursoroDbContext>(options =>
        options.UseSqlServer(xcursoroConnection));

    // Expose a scoped XcursoroDbContext created from the factory for migration operations
    builder.Services.AddScoped(sp =>
    {
        var factory = sp.GetRequiredService<IDbContextFactory<XcursoroDbContext>>();
        return factory.CreateDbContext();
    });
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddErrorDescriber<ACUEDUCTO2-0.Components.Account.Shared.SpanishIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

//builder.Services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();

builder.Services.AddScoped<SeguridadService>();
// Register migration service
//builder.Services.AddScoped<ACUEDUCTO2-0.Services.MigrationService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Ensure cookie policy is applied before authentication so correlation cookies
// set by external providers are handled with the expected SameSite/Secure policy.
app.UseCookiePolicy();

// Ensure authentication middleware is registered so external auth handlers
// can process callbacks and set cookies.
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ACUEDUCTO2-0.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
// Endpoint to process external provider callbacks on the initial HTTP request.
// This runs in the HTTP pipeline (not in the Blazor circuit) so authentication
// handlers can safely register OnStarting callbacks and set cookies.
app.MapGet("/Account/ExternalLoginCallback", async (HttpContext httpContext, SignInManager<ApplicationUser> signInManager, ILogger<Program> logger, IDataProtectionProvider dataProtectionProvider) =>
{
    try
    {
        // Log request diagnostics to diagnose missing/invalid oauth state
        try
        {
            var cookies = string.Join(";", httpContext.Request.Cookies.Select(kv => $"{kv.Key}={kv.Value}"));
            logger.LogInformation("External login callback: Path={Path} QueryString={QueryString} Cookies={Cookies}", httpContext.Request.Path, httpContext.Request.QueryString, cookies);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to log callback diagnostics");
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            logger.LogWarning("External login callback: unable to load external login info.");
            var status = Uri.EscapeDataString("Error loading external login information.");
            return Results.Redirect($"/Account/Login?status={status}");
        }

        // Build a minimal payload with provider and important claims
        var payload = new Dictionary<string, string?>();
        payload["provider"] = info.LoginProvider;
        var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(ClaimTypes.Name)?.Value ?? info.Principal.FindFirst("name")?.Value;
        var phone = info.Principal.FindFirst(ClaimTypes.MobilePhone)?.Value ?? info.Principal.FindFirst("phone_number")?.Value;
        if (!string.IsNullOrEmpty(email)) payload["email"] = email;
        if (!string.IsNullOrEmpty(name)) payload["name"] = name;
        if (!string.IsNullOrEmpty(phone)) payload["phone"] = phone;
        // include provider key and display name so the client can reconstruct the login info
        if (!string.IsNullOrEmpty(info.ProviderKey)) payload["providerKey"] = info.ProviderKey;
        if (!string.IsNullOrEmpty(info.ProviderDisplayName)) payload["providerDisplayName"] = info.ProviderDisplayName;

        var protector = dataProtectionProvider.CreateProtector("ACUEDUCTO2-0.ExternalLoginCookie.v1");
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var protectedBytes = protector.Protect(Encoding.UTF8.GetBytes(json));
        var protectedValue = Convert.ToBase64String(protectedBytes);

        // Set a short-lived, secure cookie containing the protected payload
        httpContext.Response.Cookies.Append("ExternalLoginInfo", protectedValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(5)
        });

        var returnUrl = httpContext.Request.Query["returnUrl"].ToString();
        var redirect = "/Account/ExternalLogin?Action=LoginCallback";
        if (!string.IsNullOrEmpty(returnUrl))
        {
            redirect += "&ReturnUrl=" + Uri.EscapeDataString(returnUrl);
        }

        return Results.Redirect(redirect);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing external login callback endpoint.");
        var status = Uri.EscapeDataString("Error loading external login information.");
        return Results.Redirect($"/Account/Login?status={status}");
    }
});

app.MapFallbackToFile("index.html");

// Admin endpoint to trigger migration from xcursoro_acueducto to ACUBASE
//app.MapPost("/admin/migrate-predios", async (ACUEDUCTO2-0.Services.MigrationService migrator, ILogger<Program> logger, CancellationToken ct) =>
//{
//    try
//    {
//        if (migrator is null)
//        {
//            logger.LogError("Migration service not available");
//            return Results.Problem("Migration service not configured.");
//        }

//        var count = await migrator.MigratePrediosAsync(ct);
//        return Results.Ok(new { MigratedPredios = count });
//    }
//    catch (OperationCanceledException)
//    {
//        return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error running migration");
//        return Results.Problem("Migration failed. See server logs.");
//    }
//});

app.MapPost("/api/consumptions", async (ConsumptionData data) =>
{
    // Simple calculation for demo
    double medicionAnterior = 0; // Assume 0 for demo
    double consumoM3 = data.MedicionActual - medicionAnterior;
    if (consumoM3 < 0) consumoM3 = 0;

    // Hardcoded tarifas for demo
    double valorConsumo = consumoM3 * 1000; // Example

    // In real, save to database

    return Results.Ok(new { Message = "Consumption registered successfully", ConsumoM3 = consumoM3, ValorConsumo = valorConsumo });
});

// Ensure Seguridad table exists with role-based schema
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Seguridad')
        BEGIN
            CREATE TABLE [Seguridad] (
                [Guid] NVARCHAR(450) NOT NULL,
                [IdRol] NVARCHAR(450) NULL,
                [Proceso] NVARCHAR(450) NULL,
                [Observaciones] NVARCHAR(MAX) NULL,
                [FechaHoraCreacionRegistro] DATETIME2 NOT NULL,
                [UsuarioSistemaCreaRegistro] NVARCHAR(450) NULL,
                CONSTRAINT [PK_Seguridad] PRIMARY KEY ([Guid])
            )
        END");

    // Migrate from user-based to role-based: add IdRol column if missing
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Seguridad') AND name = 'IdRol')
        BEGIN
            ALTER TABLE [Seguridad] ADD [IdRol] NVARCHAR(450) NULL
        END");

    // Drop the incorrect FK that references dbo.Usuario instead of AspNetUsers
    // (must happen before dropping IdUsuario column)
    db.Database.ExecuteSqlRaw(@"
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Seguridad_Usuario')
        BEGIN
            ALTER TABLE [Seguridad] DROP CONSTRAINT [FK_Seguridad_Usuario]
        END");

    // Remove old IdUsuario column if it exists (user-based security replaced by role-based)
    db.Database.ExecuteSqlRaw(@"
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Seguridad') AND name = 'IdUsuario')
        BEGIN
            IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Seguridad_IdUsuario' AND object_id = OBJECT_ID('Seguridad'))
            BEGIN
                DROP INDEX [IX_Seguridad_IdUsuario] ON [Seguridad]
            END
            -- Drop any default constraint on IdUsuario
            DECLARE @defName NVARCHAR(256)
            SELECT @defName = dc.name
            FROM sys.default_constraints dc
            JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
            WHERE dc.parent_object_id = OBJECT_ID('Seguridad') AND c.name = 'IdUsuario'
            IF @defName IS NOT NULL
                EXEC('ALTER TABLE [Seguridad] DROP CONSTRAINT [' + @defName + ']')
            DELETE FROM [Seguridad]
            ALTER TABLE [Seguridad] DROP COLUMN [IdUsuario]
        END");

    // Create Proceso table if it does not exist
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Proceso')
        BEGIN
            CREATE TABLE [Proceso] (
                [Id] NVARCHAR(450) NOT NULL,
                [Objeto] NVARCHAR(450) NULL,
                [Nombre] NVARCHAR(450) NULL,
                [IdTipoProceso] INT NULL,
                CONSTRAINT [PK_Proceso] PRIMARY KEY ([Id])
            )
        END");

    // Migrate Id column from INT to NVARCHAR(450) if table was created with older schema
    db.Database.ExecuteSqlRaw(@"
        IF EXISTS (
            SELECT * FROM sys.columns
            WHERE object_id = OBJECT_ID('Proceso') AND name = 'Id'
              AND system_type_id = (SELECT system_type_id FROM sys.types WHERE name = 'int')
        )
        BEGIN
            DELETE FROM [Proceso]
            ALTER TABLE [Proceso] DROP CONSTRAINT [PK_Proceso]
            ALTER TABLE [Proceso] DROP COLUMN [Id]
            ALTER TABLE [Proceso] ADD [Id] NVARCHAR(450) NOT NULL DEFAULT NEWID()
            ALTER TABLE [Proceso] ADD CONSTRAINT [PK_Proceso] PRIMARY KEY ([Id])
        END");

    // Add missing columns to Proceso if they don't exist (table was created with older schema)
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proceso') AND name = 'Objeto')
        BEGIN
            ALTER TABLE [Proceso] ADD [Objeto] NVARCHAR(450) NULL
        END");

    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proceso') AND name = 'IdTipoProceso')
        BEGIN
            IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proceso') AND name = 'IdTipoProceso')
            BEGIN
                ALTER TABLE [Proceso] ADD [IdTipoProceso] INT NULL
                UPDATE [Proceso] SET [IdTipoProceso] = CASE WHEN [IdTipoProceso] = N'EDITAR' THEN 1 WHEN [IdTipoProceso] IN (N'INFORMACION', N'VER') THEN 2 ELSE NULL END
                ALTER TABLE [Proceso] DROP COLUMN [IdTipoProceso]
            END
            ELSE
                ALTER TABLE [Proceso] ADD [IdTipoProceso] INT NULL
        END");

    // Seed Proceso table from ProcesosCatalogo
    var existingCount = db.Proceso.Count();
    if (existingCount == 0)
    {
        foreach (var def in ProcesosCatalogo.Todos)
        {
            db.Proceso.Add(new Proceso
            {
                Id = System.Guid.NewGuid().ToString(),
                Objeto = def.Objeto,
                Nombre = def.Nombre,
                IdTipoProceso = def.Tipo
            });
        }
        db.SaveChanges();
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Create "Administradores" role if it does not exist
    if (!await roleManager.RoleExistsAsync("Administradores"))
    {
        await roleManager.CreateAsync(new IdentityRole("Administradores"));
    }

    // Assign all existing users to Administradores role (initial setup)
    var users = userManager.Users.ToList();
    foreach (var user in users)
    {
        if (!await userManager.IsInRoleAsync(user, "Administradores"))
        {
            await userManager.AddToRoleAsync(user, "Administradores");
        }
    }

    db.SaveChanges();
}

app.Run();
