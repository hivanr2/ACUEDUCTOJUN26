using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class ConsumptionService
{
    private readonly HttpClient _httpClient;
    private readonly OnlineStatusService _onlineStatusService;
    private readonly OfflineQueueService _offlineQueueService;

    public ConsumptionService(HttpClient httpClient, OnlineStatusService onlineStatusService, OfflineQueueService offlineQueueService)
    {
        _httpClient = httpClient;
        _onlineStatusService = onlineStatusService;
        _offlineQueueService = offlineQueueService;

        // Subscribe to online status changes to sync when back online
        _onlineStatusService.OnlineStatusChanged += async (isOnline) =>
        {
            if (isOnline)
            {
                await SyncQueuedOperationsAsync();
            }
        };
    }

    public async Task RegisterConsumptionAsync(ConsumptionData data)
    {
        if (_onlineStatusService.IsOnline)
        {
            await SendToServerAsync(data);
        }
        else
        {
            await _offlineQueueService.EnqueueOperationAsync("RegisterConsumption", data);
        }
    }

    private async Task SendToServerAsync(ConsumptionData data)
    {
        try
        {
            // Assume the API endpoint is /api/consumptions
            var response = await _httpClient.PostAsJsonAsync("api/consumptions", data);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // On server or error, do nothing during prerendering
        }
    }

    private async Task SyncQueuedOperationsAsync()
    {
        var operations = await _offlineQueueService.GetQueuedOperationsAsync();
        foreach (var op in operations)
        {
            if (op.OperationType == "RegisterConsumption")
            {
                var data = (ConsumptionData)op.Data;
                try
                {
                    await SendToServerAsync(data);
                    // Remove from queue after success
                    await _offlineQueueService.DequeueAllAsync(); // For simplicity, dequeue all after sync
                }
                catch
                {
                    // If fails, keep in queue
                }
            }
        }
    }

    /// <summary>
    /// Obtiene los periodos disponibles para selección de consumos.
    /// En esta versión se asume un endpoint de solo lectura en el servidor.
    /// </summary>
    public async Task<List<string>> GetAvailablePeriodsAsync()
    {
        if (!_onlineStatusService.IsOnline)
        {
            return new List<string>();
        }

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<string>>("api/consumptions/periods");
            return result ?? new List<string>();
        }
        catch
        {
            // Si falla, devolvemos lista vacía sin interrumpir la captura offline.
            return new List<string>();
        }
    }

    /// <summary>
    /// Obtiene los datos de consumo (medición anterior y actual) para un predio y periodo.
    /// No modifica la lógica de cálculo en el servidor; solo consulta datos existentes.
    /// </summary>
    public async Task<ConsumptionData?> GetConsumptionForPeriodAsync(string? predioId, string? periodoId)
    {
        if (string.IsNullOrWhiteSpace(predioId) || string.IsNullOrWhiteSpace(periodoId))
        {
            return null;
        }

        if (!_onlineStatusService.IsOnline)
        {
            return null;
        }

        try
        {
            var url = $"api/consumptions/by-period?predioId={Uri.EscapeDataString(predioId)}&periodoId={Uri.EscapeDataString(periodoId)}";
            return await _httpClient.GetFromJsonAsync<ConsumptionData>(url);
        }
        catch
        {
            // Ante errores de red o servidor devolvemos null y dejamos la captura manual.
            return null;
        }
    }
}

public class ConsumptionData
{
    public string? PredioId { get; set; }
    public double MedicionActual { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Nuevos campos para soporte de UI de consumo bimestral
    public double? MedicionAnteriorM3 { get; set; }
    public double? MedicionActualM3 { get; set; }

    /// <summary>
    /// Consumo calculado en m3 (solo para visualización en UI).
    /// La lógica oficial de cálculo permanece en el servidor.
    /// </summary>
    public double? ConsumoM3 =>
        MedicionActualM3.HasValue && MedicionAnteriorM3.HasValue
            ? MedicionActualM3.Value - MedicionAnteriorM3.Value
            : null;
}