using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public interface IReportesService
{
    Task<IReadOnlyList<Factura>> GetFacturasAsync(ReportFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<Pago>> GetPagosAsync(Guid? facturaId, DateTime? desde, DateTime? hasta, CancellationToken ct = default);
    Task<byte[]> ExportarFacturasCsvAsync(ReportFilter filter, CancellationToken ct = default);
}
