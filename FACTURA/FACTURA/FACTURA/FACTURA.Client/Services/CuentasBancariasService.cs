using System.Net.Http.Json;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class CuentasBancariasService(HttpClient http) : ICuentasBancariasService
{
    private readonly HttpClient _http = http;
    private const string BaseUrl = "/api/cuentas";

    public async Task<IReadOnlyList<CuentaBancaria>> GetAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<CuentaBancaria>>(BaseUrl, ct) ?? [];

    public async Task<CuentaBancaria?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<CuentaBancaria>($"{BaseUrl}/{id}", ct);

    public async Task<CuentaBancaria?> CreateAsync(CuentaBancaria model, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(BaseUrl, model, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<CuentaBancaria>(cancellationToken: ct);
    }

    public async Task UpdateAsync(Guid id, CuentaBancaria model, CancellationToken ct = default)
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
