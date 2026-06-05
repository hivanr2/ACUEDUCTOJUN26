using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockCotizacionesService(IClientesService clientes, IProductosService productos, IFacturasService facturas) : ICotizacionesService
{
    private readonly List<Cotizacion> _cotizaciones = [];

    public async Task<IReadOnlyList<Cotizacion>> GetAsync(CancellationToken ct = default)
    {
        if (_cotizaciones.Count == 0)
        {
            var c = (await clientes.GetAsync(ct)).FirstOrDefault();
            var p = (await productos.GetAsync(ct)).FirstOrDefault();
            if (c is not null && p is not null)
            {
                _cotizaciones.Add(new Cotizacion
                {
                    Numero = "COT-0001",
                    ClienteId = c.Id,
                    ClienteNombre = c.Nombre,
                    Estado = "Enviada",
                    Items =
                    [
                        new ItemFactura { ProductoId = p.Id, ProductoNombre = p.Nombre, Cantidad = 1, PrecioUnitario = p.Precio, ImpuestoPorcentaje = p.Impuesto }
                    ]
                });
            }
        }
        return _cotizaciones.ToList();
    }

    public Task<Cotizacion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_cotizaciones.FirstOrDefault(x => x.Id == id));

    public Task<Cotizacion?> CreateAsync(Cotizacion model, CancellationToken ct = default)
    {
        model.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(model.Numero)) model.Numero = $"COT-{_cotizaciones.Count + 1:0000}";
        _cotizaciones.Add(model);
        return Task.FromResult<Cotizacion?>(model);
    }

    public Task UpdateAsync(Guid id, Cotizacion model, CancellationToken ct = default)
    {
        var idx = _cotizaciones.FindIndex(x => x.Id == id);
        if (idx >= 0)
        {
            model.Id = id;
            _cotizaciones[idx] = model;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _cotizaciones.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public async Task<Factura?> ConvertirAFacturaAsync(Guid cotizacionId, CancellationToken ct = default)
    {
        var cot = _cotizaciones.FirstOrDefault(x => x.Id == cotizacionId);
        if (cot is null) return null;
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
        return await facturas.CreateAsync(factura, ct);
    }
}
