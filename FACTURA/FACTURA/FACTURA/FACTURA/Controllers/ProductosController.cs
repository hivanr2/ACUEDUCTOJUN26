using Microsoft.AspNetCore.Mvc;
using FACTURA.Client.Models;

namespace FACTURA.Api;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private static readonly List<Producto> _db = new()
    {
        new Producto { Id = Guid.NewGuid(), Nombre = "Servicio de Soporte", Unidad = "HORA", Precio = 80000, Impuesto = 19 },
        new Producto { Id = Guid.NewGuid(), Nombre = "Hosting Mensual", Unidad = "MES", Precio = 50000, Impuesto = 0 },
        new Producto { Id = Guid.NewGuid(), Nombre = "Dominio .com", Unidad = "UND", Precio = 45000, Impuesto = 0 }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Producto>> Get() => Ok(_db);

    [HttpGet("{id:guid}")]
    public ActionResult<Producto> GetById(Guid id)
        => _db.FirstOrDefault(x => x.Id == id) is { } p ? Ok(p) : NotFound();

    [HttpPost]
    public ActionResult<Producto> Create([FromBody] Producto model)
    {
        model.Id = Guid.NewGuid();
        _db.Add(model);
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Producto model)
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
}
