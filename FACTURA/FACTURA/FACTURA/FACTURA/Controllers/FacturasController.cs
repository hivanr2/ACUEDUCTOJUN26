using Microsoft.AspNetCore.Mvc;
using FACTURA.Client.Models;

namespace FACTURA.Api;

[ApiController]
[Route("api/facturas")]
public class FacturasController : ControllerBase
{
    private static readonly List<Factura> _db = [];

    [HttpGet]
    public ActionResult<IEnumerable<Factura>> Get() => Ok(_db);

    [HttpGet("{id:guid}")]
    public ActionResult<Factura> GetById(Guid id)
        => _db.FirstOrDefault(x => x.Id == id) is { } f ? Ok(f) : NotFound();

    [HttpPost]
    public ActionResult<Factura> Create([FromBody] Factura model)
    {
        model.Id = Guid.NewGuid();
        _db.Add(model);
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Factura model)
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
