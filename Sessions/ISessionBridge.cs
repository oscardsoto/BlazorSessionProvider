namespace BlazorSessionProvider.Sessions;

/// <summary>
/// Scoped that manage all session identifiers
/// </summary>
public interface ISessionBridge
{
    /// <summary>
    /// (Async) Sets a new session id for the user
    /// </summary>
    /// <returns></returns>
    Task<string> SetNewSessionId();

    /// <summary>
    /// (Async) Gets the session id for the user
    /// </summary>
    /// <returns></returns>
    Task<string> GetSessionId();
 
    /// <summary>
    /// (Async) Removes the actual session id
    /// </summary>
    /// <returns></returns>
    Task RemoveSessionId();
}