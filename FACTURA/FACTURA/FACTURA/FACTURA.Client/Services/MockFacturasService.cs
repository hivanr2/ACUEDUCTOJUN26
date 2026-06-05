using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockFacturasService(IClientesService clientes, IProductosService productos) : IFacturasService
{
    private readonly List<Factura> _facturas = [];

    public async Task<IReadOnlyList<Factura>> GetAsync(CancellationToken ct = default)
    {
        if (_facturas.Count == 0)
        {
            var c = (await clientes.GetAsync(ct)).FirstOrDefault();
            var p = (await productos.GetAsync(ct)).FirstOrDefault();
            if (c is not null && p is not null)
            {
                _facturas.Add(new Factura
                {
                    ClienteId = c.Id,
                    ClienteNombre = c.Nombre,
                    Estado = "Emitida",
                    Items =
                    [
                        new ItemFactura { ProductoId = p.Id, ProductoNombre = p.Nombre, Cantidad = 2, PrecioUnitario = p.Precio, ImpuestoPorcentaje = p.Impuesto }
                    ]
                });
            }
        }
        return _facturas.ToList();
    }

    public Task<Factura?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_facturas.FirstOrDefault(f => f.Id == id));

    public Task<Factura?> CreateAsync(Factura model, CancellationToken ct = default)
    {
        model.Id = Guid.NewGuid();
        _facturas.Add(model);
        return Task.FromResult<Factura?>(model);
    }

    public Task UpdateAsync(Guid id, Factura model, CancellationToken ct = default)
    {
        var idx = _facturas.FindIndex(f => f.Id == id);
        if (idx >= 0)
        {
            model.Id = id;
            _facturas[idx] = model;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _facturas.RemoveAll(f => f.Id == id);
        return Task.CompletedTask;
    }
}
