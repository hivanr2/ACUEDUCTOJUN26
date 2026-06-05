using System.Text.Json;
using Microsoft.JSInterop;
using ACUEDUCTO.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace ACUEDUCTO.Services;

// Servicio simple para sincronizar Facturacion offline.
// Usa IndexedDB via JS helper (definir en wwwroot/js/offlineSync.js) para guardar:
//  - facturacionCache: lista Facturacion
//  - pendingChanges: operaciones de actualización mínima (Idfactura, MedicionActual, NotasFactura, Ruta etc.)
public class OfflineSyncService
{
    private readonly IJSRuntime _js;
    public OfflineSyncService(IJSRuntime js) { _js = js; }

    public async Task<bool> IsOnlineAsync() => await _js.InvokeAsync<bool>("offlineSync.isOnline");

    public async Task SaveFacturacionAsync(IEnumerable<Facturacion> facturas)
    {
        var json = JsonSerializer.Serialize(facturas, new JsonSerializerOptions { WriteIndented = false, ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles });
        await _js.InvokeVoidAsync("offlineSync.saveCache", json);
    }

    public record PendingChange(string Idfactura, double? MedicionActual, string? NotasFactura, double? Ruta, string? Observaciones);

    public async Task QueueChangeAsync(PendingChange change)
    {
        var json = JsonSerializer.Serialize(change);
        await _js.InvokeVoidAsync("offlineSync.queueChange", json);
    }

    public async Task<List<PendingChange>> GetPendingChangesAsync()
    {
        var json = await _js.InvokeAsync<string>("offlineSync.getPendingChanges");
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<PendingChange>>(json) ?? new();
        }
        catch { return new(); }
    }

    public async Task<List<Facturacion>> GetCachedFacturacionAsync()
    {
        var json = await _js.InvokeAsync<string>("offlineSync.getCache");
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<Facturacion>>(json) ?? new();
        }
        catch { return new(); }
    }

    public async Task ClearPendingChangesAsync() => await _js.InvokeVoidAsync("offlineSync.clearPending");
}
