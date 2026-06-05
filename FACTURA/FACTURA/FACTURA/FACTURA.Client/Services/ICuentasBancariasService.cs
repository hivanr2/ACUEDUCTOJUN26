using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public interface ICuentasBancariasService
{
    Task<IReadOnlyList<CuentaBancaria>> GetAsync(CancellationToken ct = default);
    Task<CuentaBancaria?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CuentaBancaria?> CreateAsync(CuentaBancaria model, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CuentaBancaria model, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
