using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockClientesService : IClientesService
{
    private readonly List<Cliente> _clientes =
    [
        new() { Id = Guid.NewGuid(), Nombre = "Empresa ABC", Identificacion = "900123456-7", Email = "info@abc.com", Telefono = "3001234567", Direccion = "Cra 1 # 2-3", Activo = true },
        new() { Id = Guid.NewGuid(), Nombre = "Juan Pérez", Identificacion = "1012345678", Email = "juan@example.com", Telefono = "3159876543", Direccion = "Calle 10 # 5-20", Activo = true },
        new() { Id = Guid.NewGuid(), Nombre = "Distri XYZ", Identificacion = "800987654-3", Email = "ventas@xyz.com", Telefono = "6015555544", Direccion = "Av 30 # 12-55", Activo = false }
    ];

    public Task<IReadOnlyList<Cliente>> GetAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Cliente>>(_clientes.ToList());

    public Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> CreateAsync(Cliente model, CancellationToken ct = default)
    {
        model.Id = Guid.NewGuid();
        _clientes.Add(model);
        return Task.FromResult<Cliente?>(model);
    }

    public Task UpdateAsync(Guid id, Cliente model, CancellationToken ct = default)
    {
        var idx = _clientes.FindIndex(c => c.Id == id);
        if (idx >= 0)
        {
            model.Id = id;
            _clientes[idx] = model;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _clientes.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}
