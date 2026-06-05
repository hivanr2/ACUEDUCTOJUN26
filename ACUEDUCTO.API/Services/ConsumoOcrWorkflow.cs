using ACUEDUCTO.COMPARTIR.DTO;
using ACUEDUCTO.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO.API.Services;

public sealed class ConsumoOcrWorkflow
{
    private readonly IDbContextFactory<AcueductoContext> _dbFactory;

    public ConsumoOcrWorkflow(IDbContextFactory<AcueductoContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<ConsumoOcrAutorizarResponse> AutorizarAsync(
        ConsumoOcrAutorizarRequest req,
        CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var predio = await db.Predios.AsNoTracking().FirstOrDefaultAsync(p => p.Idpredio == req.PredioId, cancellationToken);
        if (predio is null)
            throw new InvalidOperationException("Predio no encontrado");

        var fechaRecibida = req.FechaRecibidaUtc.Kind == DateTimeKind.Utc
            ? req.FechaRecibidaUtc
            : DateTime.SpecifyKind(req.FechaRecibidaUtc, DateTimeKind.Utc);

        // Bimestre inmediatamente anterior: buscar la planilla con FechaHoraRegistroConsumo < fechaRecibida
        // y tomar la más reciente.
        var planilla = await db.Planillas
            .Where(p => p.FechaHoraRegistroConsumo.HasValue && p.FechaHoraRegistroConsumo.Value < fechaRecibida)
            .OrderByDescending(p => p.FechaHoraRegistroConsumo)
            .FirstOrDefaultAsync(cancellationToken);

        if (planilla is null)
            throw new InvalidOperationException("No se encontró planilla anterior para la fecha recibida");

        var factura = await db.Facturacions
            .FirstOrDefaultAsync(f => f.Idplanilla == planilla.Idplanilla && f.Idpredio == req.PredioId, cancellationToken);

        if (factura is null)
            throw new InvalidOperationException("No existe Facturacion para ese predio en la planilla anterior");

        var anterior = factura.MedicionAnterior;
        factura.MedicionActual = req.LecturaActual;

        // Recalcular consumo básico (m3) sin tocar valores monetarios aquí.
        if (factura.MedicionActual.HasValue && factura.MedicionAnterior.HasValue && factura.MedicionActual.Value >= factura.MedicionAnterior.Value)
            factura.ConsumoM3 = factura.MedicionActual.Value - factura.MedicionAnterior.Value;
        else
            factura.ConsumoM3 = 0;

        if (!string.IsNullOrWhiteSpace(req.NotasFactura))
            factura.NotasFactura = req.NotasFactura;
        if (!string.IsNullOrWhiteSpace(req.NotasRecaudo))
            factura.NotasRecaudo = req.NotasRecaudo;

        await db.SaveChangesAsync(cancellationToken);

        return new ConsumoOcrAutorizarResponse
        {
            PredioId = req.PredioId,
            PlanillaIdAplicada = planilla.Idplanilla,
            FacturaId = factura.Idfactura,
            MedicionAnterior = anterior,
            MedicionActual = req.LecturaActual,
            ConsumoM3 = factura.ConsumoM3,
            Actualizo = true
        };
    }
}
