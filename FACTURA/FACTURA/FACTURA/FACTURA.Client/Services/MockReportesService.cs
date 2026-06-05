using System.Text;
using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockReportesService : IReportesService
{
    private readonly IFacturasService _facturas;
    private readonly IPagosService _pagos;

    public MockReportesService(IFacturasService facturas, IPagosService pagos)
    {
        _facturas = facturas;
        _pagos = pagos;
    }

    public async Task<IReadOnlyList<Factura>> GetFacturasAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var all = (await _facturas.GetAsync(ct)).AsEnumerable();
        if (filter.Desde is { } d) all = all.Where(f => f.Fecha.Date >= d.Date);
        if (filter.Hasta is { } h) all = all.Where(f => f.Fecha.Date <= h.Date);
        if (filter.ClienteId is { } cid) all = all.Where(f => f.ClienteId == cid);
        if (!string.IsNullOrWhiteSpace(filter.Estado) && filter.Estado != "Todos") all = all.Where(f => f.Estado == filter.Estado);
        return all.OrderByDescending(f => f.Fecha).ToList();
    }

    public async Task<IReadOnlyList<Pago>> GetPagosAsync(Guid? facturaId, DateTime? desde, DateTime? hasta, CancellationToken ct = default)
    {
        var facs = await _facturas.GetAsync(ct);
        var pagosList = new List<Pago>();
        foreach (var f in facs)
        {
            if (facturaId.HasValue && f.Id != facturaId.Value) continue;
            var ps = await _pagos.GetByFacturaAsync(f.Id, ct);
            pagosList.AddRange(ps);
        }
        var q = pagosList.AsEnumerable();
        if (desde is { } d) q = q.Where(p => p.Fecha.Date >= d.Date);
        if (hasta is { } h) q = q.Where(p => p.Fecha.Date <= h.Date);
        return q.OrderByDescending(p => p.Fecha).ToList();
    }

    public async Task<byte[]> ExportarFacturasCsvAsync(ReportFilter filter, CancellationToken ct = default)
    {
        var list = await GetFacturasAsync(filter, ct);
        var sb = new StringBuilder();
        sb.AppendLine("Fecha,Cliente,Estado,Subtotal,Impuestos,Total");
        foreach (var f in list)
        {
            sb.AppendLine($"{f.Fecha:yyyy-MM-dd},{Escape(f.ClienteNombre)},{f.Estado},{f.Subtotal},{f.Impuestos},{f.Total}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string? s) => string.IsNullOrEmpty(s) ? "" : '"' + s.Replace("\"", "\"\"") + '"';
}
