
using Microsoft.JSInterop;
using Microsoft.Extensions.Options;

namespace BlazorSessionProvider.Sessions;

/// <summary>
/// Scoped that manage all session identifiers
/// </summary>
public class SessionBridge : ISessionBridge
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SessionProviderConfig _config;

    /// <summary>
    /// Scoped that provide the session for the user
    /// </summary>
    public SessionBridge(IJSRuntime jsRuntime, IOptions<SessionProviderConfig> options)
    {
        _jsRuntime  = jsRuntime;
        _config     = options.Value;
    }

    /// <summary>
    /// Gets the session Guid for the user
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetSessionId() => await _jsRuntime.InvokeAsync<string>("localStorage.getItem", _config.KeyId);

    /// <summary>
    /// Removes the actual session Guid
    /// </summary>
    /// <returns></returns>
    public async Task RemoveSessionId() => await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", _config.KeyId);

    /// <summary>
    /// Sets a new session Guid for the user
    /// </summary>
    /// <returns></returns>
    public async Task<string> SetNewSessionId()
    {
        string sessionId = Guid.NewGuid().ToString();
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _config.KeyId, sessionId);
        return sessionId;
    }
}