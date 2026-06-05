using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ACUEDUCTO2.Components;
using ACUEDUCTO2.Client;
using ACUEDUCTO2.Components.Account;

namespace ACUEDUCTO2.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
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
        app.UseCookiePolicy();

        // Serve Blazor WebAssembly static assets and regular static files
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        // Endpoint routing must be configured before calling UseAuthentication/UseAuthorization
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.UseEndpoints(endpoints =>
        {
            // Map Razor Components (App) so server-side interactive components are available
            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(ACUEDUCTO2.Client._Imports).Assembly);

            // Map additional identity endpoints provided by the components package
            endpoints.MapAdditionalIdentityEndpoints();

            // Fallback to the Blazor client index.html for SPA routing
            endpoints.MapFallbackToFile("index.html");
        });

        return app;
    }
}
