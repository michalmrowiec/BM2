using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BM2.Client.Services;

/// <summary>Per-device preferences in localStorage; unavailable storage (private mode) is not an error.</summary>
public sealed partial class BrowserStorage(IJSRuntime js, ILogger<BrowserStorage> logger)
{
    public async ValueTask<string?> GetAsync(string key)
    {
        try
        {
            return await js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSException e)
        {
            LogUnavailable(logger, e);
            return null;
        }
    }

    public async ValueTask SetAsync(string key, string value)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException e)
        {
            LogUnavailable(logger, e);
        }
    }

    public async ValueTask RemoveAsync(string key)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (JSException e)
        {
            LogUnavailable(logger, e);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "localStorage is unavailable")]
    private static partial void LogUnavailable(ILogger logger, Exception exception);
}
