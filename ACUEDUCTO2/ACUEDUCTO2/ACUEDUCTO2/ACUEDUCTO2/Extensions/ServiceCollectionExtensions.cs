using ACUEDUCTO2.Components; // for IdentityRevalidatingAuthenticationStateProvider
using ACUEDUCTO2.Components.Account; // for IdentityUserAccessor, IdentityRedirectManager
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Services.ConsumoServices;
using ACUEDUCTO2.Services;
using ACUEDUCTO2.Services.Email; // Added correct namespace for SmtpEmailSender and IEmailSender
using ACUEDUCTO2.Services.MigrationServices;
using ACUEDUCTO2.Services.PeriodoServices;
using ACUEDUCTO2.Services.PredioServices;
using ACUEDUCTO2.Services.ProcesoServices;
using Microsoft.AspNetCore.Components; // for AddRazorComponents extensions
using Microsoft.AspNetCore.Components.Authorization; // for AuthenticationStateProvider
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Data Protection
        var keysFolder = Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys");
        Directory.CreateDirectory(keysFolder);
        services.AddDataProtection()
            .SetApplicationName("ACUEDUCTO2")
            .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

        // Add Blazor component services and authentication state serialization
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        // Authentication and Identity
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddFacebook(options =>
            {
                options.AppId = configuration["Authentication:Facebook:AppId"]!;
                options.AppSecret = configuration["Authentication:Facebook:AppSecret"]!;
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddIdentityCookies();

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.Secure = CookieSecurePolicy.Always;
        });




        // Ensure Identity's application and external cookies use SameSite=None so
        // correlation cookies used during external OAuth flows are not blocked by browsers.
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // Database Contexts
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            return factory.CreateDbContext();
        });

        var xcursoroConnection = configuration.GetConnectionString("AcueductoV1");
        if (!string.IsNullOrEmpty(xcursoroConnection))
        {
            services.AddDbContextFactory<XcursoroDbContext>(options =>
                options.UseSqlServer(xcursoroConnection));

            services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<XcursoroDbContext>>();
                return factory.CreateDbContext();
            });
        }

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddErrorDescriber<ACUEDUCTO2.Components.Account.Shared.SpanishIdentityErrorDescriber>()
            .AddDefaultTokenProviders();

        // Application Services
        // Registrar configuración de SmtpEmailOptions
        services.Configure<SmtpEmailOptions>(configuration.GetSection("SmtpEmail"));

        // Registrar SmtpEmailSender (qualify types to avoid ambiguity with other SmtpEmailSender implementations)
        services.AddTransient<ACUEDUCTO2.Services.Email.CustomEmailSender<ACUEDUCTO2.Data.ApplicationUser>, ACUEDUCTO2.Services.Email.SmtpEmailSender>();
        // Register the Identity-compatible generic email sender so Blazor Identity components
        // that inject Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser> will be satisfied.
        services.AddScoped<Microsoft.AspNetCore.Identity.IEmailSender<ACUEDUCTO2.Data.ApplicationUser>, ACUEDUCTO2.Components.Account.SmtpEmailSender>();
        services.AddScoped<SeguridadService>();
        services.AddScoped<ProcesoService>();
        services.AddScoped<PeriodoService>();
        services.AddScoped<PredioService>();
        services.AddScoped<ConsumoService>();
        services.AddScoped<MigrationService>();
        services.AddScoped<EmailService>();
        services.AddScoped<IPdfService, HtmlPdfService>();
        services.AddScoped<ConsumoOcrAppService>();
        services.AddScoped<IConsumoOcrService, NoopConsumoOcrService>();
        services.AddScoped<IWhatsAppService, NoopWhatsAppService>();
        // Menu permission service used by layouts/components to control footer visibility
        services.AddScoped<IMenuPermissionService, MenuPermissionService>();

        // Agregar servicios de autorización
        services.AddAuthorization();

        // Agregar servicios de antifalsificación
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN"; // Configurar el nombre del encabezado para el token CSRF
        });

        return services;
    }
}
