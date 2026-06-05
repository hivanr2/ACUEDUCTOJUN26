using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public interface ICotizacionesService
{
    Task<IReadOnlyList<Cotizacion>> GetAsync(CancellationToken ct = default);
    Task<Cotizacion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Cotizacion?> CreateAsync(Cotizacion model, CancellationToken ct = default);
    Task UpdateAsync(Guid id, Cotizacion model, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Factura?> ConvertirAFacturaAsync(Guid cotizacionId, CancellationToken ct = default);
}
