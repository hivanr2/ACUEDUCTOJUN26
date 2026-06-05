using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockProductosService : IProductosService
{
    private readonly List<Producto> _productos =
    [
        new() { Id = Guid.NewGuid(), Nombre = "Servicio de Soporte", Unidad = "HORA", Precio = 80000, Impuesto = 19 },
        new() { Id = Guid.NewGuid(), Nombre = "Hosting Mensual", Unidad = "MES", Precio = 50000, Impuesto = 0 },
        new() { Id = Guid.NewGuid(), Nombre = "Dominio .com", Unidad = "UND", Precio = 45000, Impuesto = 0, Activo = true }
    ];

    public Task<IReadOnlyList<Producto>> GetAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Producto>>(_productos.ToList());

    public Task<Producto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_productos.FirstOrDefault(p => p.Id == id));

    public Task<Producto?> CreateAsync(Producto model, CancellationToken ct = default)
    {
        model.Id = Guid.NewGuid();
        _productos.Add(model);
        return Task.FromResult<Producto?>(model);
    }

    public Task UpdateAsync(Guid id, Producto model, CancellationToken ct = default)
    {
        var idx = _productos.FindIndex(p => p.Id == id);
        if (idx >= 0)
        {
            model.Id = id;
            _productos[idx] = model;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _productos.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
