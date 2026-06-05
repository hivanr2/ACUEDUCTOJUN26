using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public interface IProductosService
{
    Task<IReadOnlyList<Producto>> GetAsync(CancellationToken ct = default);
    Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Producto?> CreateAsync(Producto model, CancellationToken ct = default);
    Task UpdateAsync(Guid id, Producto model, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
