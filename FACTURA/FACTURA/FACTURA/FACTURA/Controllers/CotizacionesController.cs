using Microsoft.AspNetCore.Mvc;
using FACTURA.Client.Models;

namespace FACTURA.Api;

[ApiController]
[Route("api/cotizaciones")]
public class CotizacionesController : ControllerBase
{
    private static readonly List<Cotizacion> _db = [];

    [HttpGet]
    public ActionResult<IEnumerable<Cotizacion>> Get() => Ok(_db);

    [HttpGet("{id:guid}")]
    public ActionResult<Cotizacion> GetById(Guid id)
        => _db.FirstOrDefault(x => x.Id == id) is { } c ? Ok(c) : NotFound();

    [HttpPost]
    public ActionResult<Cotizacion> Create([FromBody] Cotizacion model)
    {
        model.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(model.Numero)) model.Numero = $"COT-{_db.Count + 1:0000}";
        _db.Add(model);
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Cotizacion model)
    {
        var idx = _db.FindIndex(x => x.Id == id);
        if (idx < 0) return NotFound();
        model.Id = id;
        _db[idx] = model;
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var removed = _db.RemoveAll(x => x.Id == id) > 0;
        return removed ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/convertir")]
    public ActionResult<Factura> Convertir(Guid id)
    {
        var cot = _db.FirstOrDefault(x => x.Id == id);
        if (cot is null) return NotFound();
        cot.Estado = "Convertida";
        var factura = new Factura
        {
            ClienteId = cot.ClienteId,
            ClienteNombre = cot.ClienteNombre,
            Fecha = DateTime.Today,
            Estado = "Emitida",
            Items = cot.Items.Select(i => new ItemFactura
            {
                ProductoId = i.ProductoId,
                ProductoNombre = i.ProductoNombre,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                ImpuestoPorcentaje = i.ImpuestoPorcentaje
            }).ToList()
        };
        return Ok(factura);
    }
}
