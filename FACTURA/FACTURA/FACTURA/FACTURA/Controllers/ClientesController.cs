using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FACTURA.Api;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private static readonly List<ClienteDto> _db = new()
    {
        new ClienteDto{ Id = Guid.NewGuid(), Nombre="Empresa ABC", Identificacion="900123456-7", Email="info@abc.com", Telefono="3001234567", Direccion="Cra 1 # 2-3", Activo = true },
        new ClienteDto{ Id = Guid.NewGuid(), Nombre="Juan Pérez", Identificacion="1012345678", Email="juan@example.com", Telefono="3159876543", Direccion="Calle 10 # 5-20", Activo = true }
    };

    [HttpGet]
    public ActionResult<IEnumerable<ClienteDto>> Get() => Ok(_db);

    [HttpGet("{id:guid}")]
    public ActionResult<ClienteDto> GetById(Guid id)
        => _db.FirstOrDefault(x => x.Id == id) is { } c ? Ok(c) : NotFound();

    [HttpPost]
    public ActionResult<ClienteDto> Create([FromBody] ClienteDto dto)
    {
        dto.Id = Guid.NewGuid();
        _db.Add(dto);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] ClienteDto dto)
    {
        var idx = _db.FindIndex(x => x.Id == id);
        if (idx < 0) return NotFound();
        dto.Id = id;
        _db[idx] = dto;
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var removed = _db.RemoveAll(x => x.Id == id) > 0;
        return removed ? NoContent() : NotFound();
    }

    public class ClienteDto
    {
        public Guid Id { get; set; }
        [Required] public string Nombre { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        [EmailAddress] public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
