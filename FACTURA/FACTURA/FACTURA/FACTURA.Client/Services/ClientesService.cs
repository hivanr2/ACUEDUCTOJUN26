using System.Net.Http.Json;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class ClientesService(HttpClient http) : IClientesService
{
    private readonly HttpClient _http = http;

    private const string BaseUrl = "/api/clientes";

    public async Task<IReadOnlyList<Cliente>> GetAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<Cliente>>(BaseUrl, ct) ?? [];

    public async Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<Cliente>($"{BaseUrl}/{id}", ct);

    public async Task<Cliente?> CreateAsync(Cliente model, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(BaseUrl, model, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<Cliente>(cancellationToken: ct);
    }

    public async Task UpdateAsync(Guid id, Cliente model, CancellationToken ct = default)
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
