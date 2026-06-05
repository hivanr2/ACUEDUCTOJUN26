using FACTURA.Client.Models;

namespace FACTURA.Client.Services;

public class MockCuentasBancariasService : ICuentasBancariasService
{
    private readonly List<CuentaBancaria> _cuentas =
    [
        new() { Id = Guid.NewGuid(), Banco = "Bancolombia", Numero = "123-456789-01", Alias = "Principal" },
        new() { Id = Guid.NewGuid(), Banco = "Davivienda", Numero = "987-654321-00", Alias = "Ahorros" }
    ];

    public Task<IReadOnlyList<CuentaBancaria>> GetAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CuentaBancaria>>(_cuentas.ToList());

    public Task<CuentaBancaria?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_cuentas.FirstOrDefault(c => c.Id == id));

    public Task<CuentaBancaria?> CreateAsync(CuentaBancaria model, CancellationToken ct = default)
    {
        model.Id = Guid.NewGuid();
        _cuentas.Add(model);
        return Task.FromResult<CuentaBancaria?>(model);
    }

    public Task UpdateAsync(Guid id, CuentaBancaria model, CancellationToken ct = default)
    {
        var idx = _cuentas.FindIndex(c => c.Id == id);
        if (idx >= 0)
        {
            model.Id = id;
            _cuentas[idx] = model;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _cuentas.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}
