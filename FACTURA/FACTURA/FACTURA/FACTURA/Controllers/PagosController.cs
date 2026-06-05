using Microsoft.AspNetCore.Mvc;
using FACTURA.Client.Models;

namespace FACTURA.Api;

[ApiController]
[Route("api/pagos")]
public class PagosController : ControllerBase
{
    private static readonly List<Pago> _db = [];

    [HttpGet("factura/{facturaId:guid}")]
    public ActionResult<IEnumerable<Pago>> GetByFactura(Guid facturaId)
        => Ok(_db.Where(x => x.FacturaId == facturaId).OrderByDescending(x => x.Fecha));

    [HttpPost]
    public ActionResult<Pago> Create([FromBody] Pago model)
    {
        model.Id = Guid.NewGuid();
        _db.Add(model);
        return CreatedAtAction(nameof(GetByFactura), new { facturaId = model.FacturaId }, model);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var removed = _db.RemoveAll(x => x.Id == id) > 0;
        return removed ? NoContent() : NotFound();
    }
}
