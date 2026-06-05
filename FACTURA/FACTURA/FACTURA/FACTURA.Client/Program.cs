using Blazored.LocalStorage;
using FACTURA.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace FACTURA.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthenticationStateDeserialization();

            // HttpClient para API del host
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Toggle de servicio: Mock o API real
            var useMock = builder.Configuration.GetValue("UseMockData", true);
            if (useMock)
            {
                builder.Services.AddSingleton<IClientesService, MockClientesService>();
                builder.Services.AddSingleton<IProductosService, MockProductosService>();
                builder.Services.AddSingleton<IFacturasService, MockFacturasService>();
                builder.Services.AddSingleton<ICotizacionesService, MockCotizacionesService>();
                builder.Services.AddSingleton<IPagosService, MockPagosService>();
                builder.Services.AddSingleton<ICuentasBancariasService, MockCuentasBancariasService>();
                builder.Services.AddSingleton<IConfiguracionService, MockConfiguracionService>();
                builder.Services.AddSingleton<IReportesService, MockReportesService>();
            }
            else
            {
                builder.Services.AddScoped<IClientesService, ClientesService>();
                builder.Services.AddScoped<IProductosService, ProductosService>();
                builder.Services.AddScoped<IFacturasService, FacturasService>();
                builder.Services.AddScoped<ICotizacionesService, CotizacionesService>();
                builder.Services.AddScoped<IPagosService, PagosService>();
                builder.Services.AddScoped<ICuentasBancariasService, CuentasBancariasService>();
                builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
                builder.Services.AddScoped<IReportesService, ReportesService>();
            }

            // Almacenamiento local (para tokens/config si se requiere)
            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}
