using System.Net.Http.Json;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class PagosService(HttpClient http) : IPagosService
{
    private readonly HttpClient _http = http;
    private const string BaseUrl = "/api/pagos";

    public async Task<IReadOnlyList<Pago>> GetByFacturaAsync(Guid facturaId, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<Pago>>($"{BaseUrl}/factura/{facturaId}", ct) ?? [];

    public async Task<Pago?> CreateAsync(Pago model, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(BaseUrl, model, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<Pago>(cancellationToken: ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _http.DeleteAsync($"{BaseUrl}/{id}", ct);
        res.EnsureSuccessStatusCode();
    }
}
