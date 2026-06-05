namespace FACTURA.Client.Services;

using FACTURA.Client.Models;

public interface IClientesService
{
    Task<IReadOnlyList<Cliente>> GetAsync(CancellationToken ct = default);
    Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Cliente?> CreateAsync(Cliente model, CancellationToken ct = default);
    Task UpdateAsync(Guid id, Cliente model, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
