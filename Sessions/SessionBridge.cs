
using Microsoft.JSInterop;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorSessionProvider.Sessions;

/// <summary>
/// Scoped that manage all session identifiers
/// </summary>
public class SessionBridge : ISessionBridge
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly SessionProviderConfig _config;
    private readonly Lazy<Task<IJSObjectReference>> _cookiesModule;
    
    /// <summary>
    /// Scoped that provide the session for the user
    /// </summary>
    public SessionBridge(IJSRuntime jsRuntime, IOptions<SessionProviderConfig> options, IHttpContextAccessor httpContext)
    {
        _config         = options.Value;
        _httpContext    = httpContext;
        _cookiesModule  = new (() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorSessionProvider/bspCookies.js").AsTask());
    }

    /// <inheritdoc/>
    public async Task<string> GetSessionId()
    {
        var context = _httpContext.HttpContext;
        if (context != null)
        {
            // Does not need try-catch. Exception only happens on Response
            if (context.Request.Cookies.ContainsKey(_config.KeyId))
                return context.Request.Cookies[_config.KeyId]!;
            return "";
        }

        if (_config.UseHttpOnlyCookies)
            throw new InvalidOperationException("Invalid request: Unable to get an HttpOnly cookie from the current context due to the 'UseHttpOnlyCookies' configuration.");

        var module = await _cookiesModule.Value;
        string cookiesValue = await module.InvokeAsync<string>("getCookiesValue");
        return GetCookieByName(cookiesValue, _config.KeyId);
    }

    /// <inheritdoc/>
    public async Task RemoveSessionId()
    {
        var context = _httpContext.HttpContext;
        if (context != null)
        {
            try
            {
                context.Response.Cookies.Delete(_config.KeyId);
                return;
            }
            catch { }
            // Ignore the exception on purpose, try again in JS
        }

        // If you specify HttpOnly, then its wrong
        if (_config.UseHttpOnlyCookies)
            throw new InvalidOperationException("Invalid request: Unable to remove an HttpOnly cookie from the current context due to the 'UseHttpOnlyCookies' configuration.");

        var module = await _cookiesModule.Value;
        await module.InvokeVoidAsync("removeCookie", _config.KeyId);
    }

    /// <inheritdoc/>
    public async Task<string> SetNewSessionId()
    {
        string sessionId = Guid.NewGuid().ToString();
        var context = _httpContext.HttpContext;
        if (context != null)
        {
            try
            {
                context.Response.Cookies.Append(_config.KeyId, sessionId, new CookieOptions
                {
                    HttpOnly = _config.UseHttpOnlyCookies,
                    Secure = _config.UseHttpOnlyCookies,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                });
                return sessionId;
            }
            catch { }
            // Ignore the exception on purpose, try again in JS
        }
        
        // If you specify HttpOnly, then its wrong
        if (_config.UseHttpOnlyCookies)
            throw new InvalidOperationException("Invalid request: Unable to append an HttpOnly cookie from the current context due to the 'UseHttpOnlyCookies' configuration.");

        var module = await _cookiesModule.Value;
        await module.InvokeVoidAsync("addCookie", _config.KeyId, sessionId);
        return sessionId;
    }
    
    /// <summary>
    /// Gets the cookie value by it's key name
    /// </summary>
    /// <param name="cookieString">The entire string value for the cookie</param>
    /// <param name="name">Name to search in the string</param>
    /// <returns></returns>
    string GetCookieByName(string cookieString, string name)
    {
        var pattern = @"(^| )" + Regex.Escape(name) + @"=([^;]+)";
        var match = Regex.Match(cookieString, pattern);

        if (match.Success)
            return match.Groups[2].Value;
        return "";
    }
}