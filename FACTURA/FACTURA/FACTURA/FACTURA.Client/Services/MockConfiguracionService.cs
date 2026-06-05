using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockConfiguracionService : IConfiguracionService
{
    private EmpresaConfig _config = new()
    {
        Nombre = "FacturaPro SAS",
        NIT = "901234567-8",
        Direccion = "Calle 100 # 10-20",
        Telefono = "+57 601 555 5555",
        Email = "info@facturapro.com",
        ImpuestoPorDefecto = 19m,
        ConsecutivoFacturaPrefijo = "FAC-",
        ConsecutivoFacturaSiguiente = 1001,
        ConsecutivoCotizacionPrefijo = "COT-",
        ConsecutivoCotizacionSiguiente = 2001
    };

    public Task<EmpresaConfig> GetAsync(CancellationToken ct = default) => Task.FromResult(_config);
    public Task<EmpresaConfig> SaveAsync(EmpresaConfig model, CancellationToken ct = default)
    {
        _config = model;
        return Task.FromResult(_config);
    }
}
