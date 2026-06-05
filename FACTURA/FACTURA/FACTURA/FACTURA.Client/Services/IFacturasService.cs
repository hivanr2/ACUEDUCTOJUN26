using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public interface IFacturasService
{
    Task<IReadOnlyList<Factura>> GetAsync(CancellationToken ct = default);
    Task<Factura?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Factura?> CreateAsync(Factura model, CancellationToken ct = default);
    Task UpdateAsync(Guid id, Factura model, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
