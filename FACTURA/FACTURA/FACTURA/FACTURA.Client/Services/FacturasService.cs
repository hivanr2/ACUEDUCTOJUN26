using System.Net.Http.Json;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class FacturasService(HttpClient http) : IFacturasService
{
    private readonly HttpClient _http = http;
    private const string BaseUrl = "/api/facturas";

    public async Task<IReadOnlyList<Factura>> GetAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<Factura>>(BaseUrl, ct) ?? [];

    public async Task<Factura?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<Factura>($"{BaseUrl}/{id}", ct);

    public async Task<Factura?> CreateAsync(Factura model, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(BaseUrl, model, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<Factura>(cancellationToken: ct);
    }

    public async Task UpdateAsync(Guid id, Factura model, CancellationToken ct = default)
    {
        var res = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", model, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _http.DeleteAsync($"{BaseUrl}/{id}", ct);
        res.EnsureSuccessStatusCode();
    }
}
