using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ACUEDUCTO.Services;

public class CircuitStateService : CircuitHandler
{
    private CancellationTokenSource _cts = new();

    public CancellationToken CancellationToken => _cts.Token;

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        return Task.CompletedTask;
    }
}