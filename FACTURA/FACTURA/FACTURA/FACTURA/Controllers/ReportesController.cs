using Microsoft.AspNetCore.Mvc;
using FACTURA.Client.Models;
using FACTURA.Client.Services; // usar contratos del cliente para reusar lógica (mock temporal)

namespace FACTURA.Api;

[ApiController]
[Route("api/reportes")] 
public class ReportesController(IReportesService reportes) : ControllerBase
{
    private readonly IReportesService _reportes = reportes;

    [HttpPost("facturas")]
    public async Task<ActionResult<IEnumerable<Factura>>> Facturas([FromBody] ReportFilter filter, CancellationToken ct)
        => Ok(await _reportes.GetFacturasAsync(filter, ct));

    [HttpGet("pagos")]
    public async Task<ActionResult<IEnumerable<Pago>>> Pagos([FromQuery] Guid? facturaId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, CancellationToken ct)
        => Ok(await _reportes.GetPagosAsync(facturaId, desde, hasta, ct));

    [HttpPost("facturas/csv")]
    public async Task<IActionResult> FacturasCsv([FromBody] ReportFilter filter, CancellationToken ct)
    {
        var bytes = await _reportes.ExportarFacturasCsvAsync(filter, ct);
        return File(bytes, "text/csv", $"facturas_{DateTime.Now:yyyyMMddHHmmss}.csv");
    }
}
