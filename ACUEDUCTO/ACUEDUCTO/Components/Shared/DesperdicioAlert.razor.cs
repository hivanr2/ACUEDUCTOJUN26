using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACUEDUCTO.Models;
using ACUEDUCTO.COMPARTIR.DTO;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO.Components.Shared;

public partial class DesperdicioAlert
{
    private bool isLoading = true;

    private const int Window = DesperdicioConfig.Window;

    private List<PredioAlert> predioAlerts = new();
    private int totalPrediosCount = 0;

    // Public accessor to obtain the summary (count of alerts, total predios)
    public async Task<(int Count, int Total)> GetSummaryAsync()
    {
        if (isLoading)
        {
            // ensure data load finishes
            await LoadAsync();
        }
        return (predioAlerts?.Count ?? 0, totalPrediosCount);
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        isLoading = true;
        try
        {
            using var db = DbFactory.CreateDbContext();

            // Load predios with their facturaciones and planilla (periodo/date)
            var predios = await db.Predios
                .Include(p => p.Facturacions) // Facturacion tiene referencia a Planilla
                    .ThenInclude(f => f.IdplanillaNavigation)
                .ToListAsync();

            totalPrediosCount = predios.Count;
            var list = new List<PredioAlert>();

            foreach (var pred in predios)
            {
                // Consider only the most recent 6 bimestres
                var facts = pred.Facturacions?
                    .Where(f => f.ConsumoM3.HasValue && f.ConsumoM3 > 0 && f.IdplanillaNavigation != null && f.IdplanillaNavigation.FechaHoraFacturacion != null)
                    .OrderByDescending(f => f.IdplanillaNavigation!.FechaHoraFacturacion)
                    .Take(DesperdicioConfig.Window)
                .Take(Window)
                    .ToList() ?? new List<Facturacion>();

                if (facts.Count == 0)
                    continue;

                var promedio = facts.Average(f => f.ConsumoM3 ?? 0.0);
                // Redondear el promedio hacia arriba para la comparación
                var promedioRedondeado = Math.Ceiling(promedio);

                // Incluir solo bimestres cuya diferencia con el promedio redondeado
                // sea al menos 2 m3 (si es igual o 1 m3 por encima, no se incluye)
                // Determine which records are above threshold inside the window
                var aboveSet = new HashSet<string>(); // Idfactura set for quick lookup
                var aboveInfos = facts
                    .Where(f => ((f.ConsumoM3 ?? 0.0) - promedioRedondeado) >= 2.0)
                    .OrderByDescending(f => f.IdplanillaNavigation!.FechaHoraFacturacion)
                    .ToList();

                foreach (var f in aboveInfos)
                {
                    if (!string.IsNullOrWhiteSpace(f.Idfactura)) aboveSet.Add(f.Idfactura);
                }

                // Only consider this predio for the alert listing if the most recent bimestre
                // (within the scanned window) is itself above the threshold. In other words,
                // the offending bimestres must be consecutive backwards from the latest one.
                var mostRecent = facts.OrderByDescending(f => f.IdplanillaNavigation!.FechaHoraFacturacion).FirstOrDefault();
                if (mostRecent is not null && !string.IsNullOrWhiteSpace(mostRecent.Idfactura) && aboveSet.Contains(mostRecent.Idfactura))
                {
                    // collect consecutive above records starting from the most recent and going backwards
                    var consecutiveAbove = new List<AboveBimestreInfo>();
                    foreach (var f in facts.OrderByDescending(f => f.IdplanillaNavigation!.FechaHoraFacturacion))
                    {
                        if (!string.IsNullOrWhiteSpace(f.Idfactura) && aboveSet.Contains(f.Idfactura))
                        {
                            consecutiveAbove.Add(new AboveBimestreInfo
                            {
                                Consumo = f.ConsumoM3 ?? 0.0,
                                Fecha = f.IdplanillaNavigation?.FechaHoraFacturacion,
                                Periodo = f.IdplanillaNavigation?.IdperiodoNavigation?.DescripcionCorta ?? f.IdplanillaNavigation?.Descripcion
                            });
                        }
                        else
                        {
                            break; // stop at first non-above (break consecutiveness)
                        }
                    }

                    if (consecutiveAbove.Any())
                    {
                        list.Add(new PredioAlert
                        {
                            PredioId = pred.Idpredio,
                            PredioNombre = string.IsNullOrWhiteSpace(pred.ApellidosNombresUsuario) ? pred.CodigoUsuario ?? pred.Idpredio : pred.ApellidosNombresUsuario,
                            Promedio = promedio,
                            AboveBimestres = consecutiveAbove,
                            AboveCount = consecutiveAbove.Count
                        });
                    }
                }
            }

            predioAlerts = list.OrderByDescending(p => p.AboveCount).Take(50).ToList();
        }
        catch
        {
            // ignore errors silently to avoid breaking UI
            predioAlerts = new List<PredioAlert>();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private class PredioAlert
    {
        public string PredioId { get; set; } = string.Empty;
        public string PredioNombre { get; set; } = string.Empty;
        public double Promedio { get; set; }
        public int AboveCount { get; set; }
        public List<AboveBimestreInfo> AboveBimestres { get; set; } = new List<AboveBimestreInfo>();
    }

}
