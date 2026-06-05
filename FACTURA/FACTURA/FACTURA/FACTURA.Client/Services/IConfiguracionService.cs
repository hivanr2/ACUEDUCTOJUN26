using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public interface IConfiguracionService
{
    Task<EmpresaConfig> GetAsync(CancellationToken ct = default);
    Task<EmpresaConfig> SaveAsync(EmpresaConfig model, CancellationToken ct = default);
}
