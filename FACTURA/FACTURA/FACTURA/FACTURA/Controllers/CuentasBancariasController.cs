using Microsoft.AspNetCore.Mvc;
using FACTURA.Client.Models;

namespace FACTURA.Api;

[ApiController]
[Route("api/cuentas")]
public class CuentasBancariasController : ControllerBase
{
    private static readonly List<CuentaBancaria> _db = new()
    {
        new CuentaBancaria { Id = Guid.NewGuid(), Banco = "Bancolombia", Numero = "123-456789-01", Alias = "Principal" },
        new CuentaBancaria { Id = Guid.NewGuid(), Banco = "Davivienda", Numero = "987-654321-00", Alias = "Ahorros" }
    };

    [HttpGet]
    public ActionResult<IEnumerable<CuentaBancaria>> Get() => Ok(_db);

    [HttpGet("{id:guid}")]
    public ActionResult<CuentaBancaria> GetById(Guid id)
        => _db.FirstOrDefault(x => x.Id == id) is { } c ? Ok(c) : NotFound();

    [HttpPost]
    public ActionResult<CuentaBancaria> Create([FromBody] CuentaBancaria model)
    {
        model.Id = Guid.NewGuid();
        _db.Add(model);
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] CuentaBancaria model)
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
