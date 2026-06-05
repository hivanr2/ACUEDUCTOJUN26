using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public interface IPagosService
{
    Task<IReadOnlyList<Pago>> GetByFacturaAsync(Guid facturaId, CancellationToken ct = default);
    Task<Pago?> CreateAsync(Pago model, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
