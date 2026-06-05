using System.Net.Http.Json;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class ProductosService(HttpClient http) : IProductosService
{
    private readonly HttpClient _http = http;
    private const string BaseUrl = "/api/productos";

    public async Task<IReadOnlyList<Producto>> GetAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<Producto>>(BaseUrl, ct) ?? [];

    public async Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<Producto>($"{BaseUrl}/{id}", ct);

    public async Task<Producto?> CreateAsync(Producto model, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(BaseUrl, model, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<Producto>(cancellationToken: ct);
    }

    public async Task UpdateAsync(Guid id, Producto model, CancellationToken ct = default)
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
