using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockPagosService : IPagosService
{
    private readonly List<Pago> _pagos = [];

    public Task<IReadOnlyList<Pago>> GetByFacturaAsync(Guid facturaId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Pago>>(_pagos.Where(p => p.FacturaId == facturaId).OrderByDescending(p => p.Fecha).ToList());

    public Task<Pago?> CreateAsync(Pago model, CancellationToken ct = default)
    {
        model.Id = Guid.NewGuid();
        _pagos.Add(model);
        return Task.FromResult<Pago?>(model);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _pagos.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
