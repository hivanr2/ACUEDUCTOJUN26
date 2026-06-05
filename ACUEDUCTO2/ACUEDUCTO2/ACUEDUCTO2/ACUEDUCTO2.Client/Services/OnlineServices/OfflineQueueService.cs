using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public class OfflineQueueService
{
    private readonly IJSRuntime _jsRuntime;
    private const string QueueKey = "offlineQueue";

    public OfflineQueueService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task EnqueueOperationAsync(string operationType, object data)
    {
        var queue = await GetQueueAsync();
        queue.Add(new QueuedOperation
        {
            Id = Guid.NewGuid().ToString(),
            OperationType = operationType,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
        await SaveQueueAsync(queue);
    }

    public async Task<List<QueuedOperation>> DequeueAllAsync()
    {
        var queue = await GetQueueAsync();
        var operations = new List<QueuedOperation>(queue);
        queue.Clear();
        await SaveQueueAsync(queue);
        return operations;
    }

    public async Task<List<QueuedOperation>> GetQueuedOperationsAsync()
    {
        return await GetQueueAsync();
    }

    private async Task<List<QueuedOperation>> GetQueueAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", QueueKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<QueuedOperation>();
            }
            return JsonSerializer.Deserialize<List<QueuedOperation>>(json) ?? new List<QueuedOperation>();
        }
        catch
        {
            return new List<QueuedOperation>(); // On server, no localStorage
        }
    }

    private async Task SaveQueueAsync(List<QueuedOperation> queue)
    {
        try
        {
            var json = JsonSerializer.Serialize(queue);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", QueueKey, json);
        }
        catch
        {
            // On server, do nothing
        }
    }
}

public class QueuedOperation
{
    public string Id { get; set; }
    public string OperationType { get; set; }
    public object Data { get; set; }
    public DateTime Timestamp { get; set; }
}