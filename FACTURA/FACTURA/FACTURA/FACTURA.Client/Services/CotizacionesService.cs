using System.Net.Http.Json;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class CotizacionesService(HttpClient http, IFacturasService facturas) : ICotizacionesService
{
    private readonly HttpClient _http = http;
    private readonly IFacturasService _facturas = facturas;
    private const string BaseUrl = "/api/cotizaciones";

    public async Task<IReadOnlyList<Cotizacion>> GetAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<Cotizacion>>(BaseUrl, ct) ?? [];

    public async Task<Cotizacion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<Cotizacion>($"{BaseUrl}/{id}", ct);

    public async Task<Cotizacion?> CreateAsync(Cotizacion model, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(BaseUrl, model, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<Cotizacion>(cancellationToken: ct);
    }

    public async Task UpdateAsync(Guid id, Cotizacion model, CancellationToken ct = default)
    {
        var res = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", model, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _http.DeleteAsync($"{BaseUrl}/{id}", ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<Factura?> ConvertirAFacturaAsync(Guid cotizacionId, CancellationToken ct = default)
    {
        // Llama al endpoint de conversión, o como alternativa usa IFacturasService
        var res = await _http.PostAsync($"{BaseUrl}/{cotizacionId}/convertir", null, ct);
        if (res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<Factura>(cancellationToken: ct);
        }
        return null;
    }
}
