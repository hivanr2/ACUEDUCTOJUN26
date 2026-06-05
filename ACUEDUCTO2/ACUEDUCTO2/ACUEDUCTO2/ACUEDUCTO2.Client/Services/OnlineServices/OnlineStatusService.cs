using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

public class OnlineStatusService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isOnline = true;

    public event Action<bool>? OnlineStatusChanged;

    public OnlineStatusService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _isOnline = await _jsRuntime.InvokeAsync<bool>("eval", "navigator.onLine");

            // Listen to online/offline events
            await _jsRuntime.InvokeVoidAsync("eval", @"
                window.addEventListener('online', function() {
                    DotNet.invokeMethodAsync('ACUEDUCTO2.Client', 'SetOnlineStatus', true);
                });
                window.addEventListener('offline', function() {
                    DotNet.invokeMethodAsync('ACUEDUCTO2.Client', 'SetOnlineStatus', false);
                });
            ");
        }
        catch
        {
            _isOnline = true; // Assume online on server
        }
    }

    [JSInvokable]
    public void SetOnlineStatus(bool isOnline)
    {
        if (_isOnline != isOnline)
        {
            _isOnline = isOnline;
            OnlineStatusChanged?.Invoke(_isOnline);
        }
    }

    public bool IsOnline => _isOnline;
}