using System.Net.Http.Json;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class ConfiguracionService(HttpClient http) : IConfiguracionService
{
    private readonly HttpClient _http = http;
    private const string BaseUrl = "/api/config";

    public async Task<EmpresaConfig> GetAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<EmpresaConfig>(BaseUrl, ct) ?? new();

    public async Task<EmpresaConfig> SaveAsync(EmpresaConfig model, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(BaseUrl, model, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<EmpresaConfig>(cancellationToken: ct) ?? new();
    }
}
