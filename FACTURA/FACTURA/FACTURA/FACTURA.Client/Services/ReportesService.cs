using System.Net.Http.Json;
using FACTURA.Client.Models;
using System.Web;

namespace FACTURA.Client.Services;

public class ReportesService(HttpClient http) : IReportesService
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<Factura>> GetFacturasAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("/api/reportes/facturas", filter, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<IReadOnlyList<Factura>>(cancellationToken: ct) ?? [];
    }

    public async Task<IReadOnlyList<Pago>> GetPagosAsync(Guid? facturaId, DateTime? desde, DateTime? hasta, CancellationToken ct = default)
    {
        var qb = new List<string>();
        if (facturaId.HasValue) qb.Add($"facturaId={HttpUtility.UrlEncode(facturaId.Value.ToString())}");
        if (desde.HasValue) qb.Add($"desde={HttpUtility.UrlEncode(desde.Value.ToString("O"))}");
        if (hasta.HasValue) qb.Add($"hasta={HttpUtility.UrlEncode(hasta.Value.ToString("O"))}");
        var url = "/api/reportes/pagos" + (qb.Count > 0 ? ("?" + string.Join("&", qb)) : string.Empty);
        var res = await _http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<IReadOnlyList<Pago>>(cancellationToken: ct) ?? [];
    }

    public async Task<byte[]> ExportarFacturasCsvAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("/api/reportes/facturas/csv", filter, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsByteArrayAsync(ct);
    }
}
