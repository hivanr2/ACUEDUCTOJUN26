using Microsoft.AspNetCore.Mvc;
using FACTURA.Client.Models;

namespace FACTURA.Api;

[ApiController]
[Route("api/config")]
public class ConfiguracionController : ControllerBase
{
    private static EmpresaConfig _config = new()
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

    [HttpGet]
    public ActionResult<EmpresaConfig> Get() => Ok(_config);

    [HttpPost]
    public ActionResult<EmpresaConfig> Save([FromBody] EmpresaConfig model)
    {
        _config = model;
        return Ok(_config);
    }
}
