using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
// Procesos moved to server-side service. No client-side ProcesoService.

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddSingleton<OnlineStatusService>();
builder.Services.AddSingleton<OfflineQueueService>();
builder.Services.AddScoped<ConsumptionService>();

// NOTE: procesos are handled on server via ACUEDUCTO2.Services.ProcesoService

await builder.Build().RunAsync();
