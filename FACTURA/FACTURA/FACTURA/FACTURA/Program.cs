using FACTURA.Client.Pages;
using FACTURA.Components;
using FACTURA.Components.Account;
using FACTURA.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FACTURA.Client.Services;

namespace FACTURA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();

            builder.Services.AddControllers(); // API

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            // Elegir Mock o API real para prerender/SSR según configuración
            var useMock = builder.Configuration.GetValue("UseMockData", true);
            if (useMock)
            {
                builder.Services.AddScoped<IClientesService, MockClientesService>();
                builder.Services.AddScoped<IProductosService, MockProductosService>();
                builder.Services.AddScoped<IFacturasService, MockFacturasService>();
                builder.Services.AddScoped<ICotizacionesService, MockCotizacionesService>();
                builder.Services.AddScoped<IPagosService, MockPagosService>();
                builder.Services.AddScoped<ICuentasBancariasService, MockCuentasBancariasService>();
                builder.Services.AddScoped<IConfiguracionService, MockConfiguracionService>();
                builder.Services.AddScoped<IReportesService, MockReportesService>();
            }
            else
            {
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddHttpClient<IClientesService, ClientesService>(ConfigureBaseAddress);
                builder.Services.AddHttpClient<IProductosService, ProductosService>(ConfigureBaseAddress);
                builder.Services.AddHttpClient<IFacturasService, FacturasService>(ConfigureBaseAddress);
                builder.Services.AddHttpClient<ICotizacionesService, CotizacionesService>(ConfigureBaseAddress);
                builder.Services.AddHttpClient<IPagosService, PagosService>(ConfigureBaseAddress);
                builder.Services.AddHttpClient<ICuentasBancariasService, CuentasBancariasService>(ConfigureBaseAddress);
                builder.Services.AddHttpClient<IConfiguracionService, ConfiguracionService>(ConfigureBaseAddress);
                builder.Services.AddHttpClient<IReportesService, ReportesService>(ConfigureBaseAddress);
            }

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.MapControllers(); // API

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }

        private static void ConfigureBaseAddress(IServiceProvider sp, HttpClient client)
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var baseUri = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            client.BaseAddress = new Uri(baseUri);
        }
    }
}
