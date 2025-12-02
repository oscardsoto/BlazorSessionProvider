namespace BlazorSessionProvider.Sessions.Binding;

/// <summary>
/// Binds a property to a session key in a class that inherits <see cref="SessionBinder"/>
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class BindToKey : Attribute
{
    /// <summary>
    /// Session key to bind
    /// </summary>
    public string? SessionKey { get; set; }

    /// <summary>
    /// Binds a property to a session key in a class that inherits <see cref="SessionBinder"/>
    /// </summary>
    /// <param name="sessionKey">Session key to bind. Null if you want to bind a key with the same value as the property</param>
    public BindToKey(string? sessionKey = null)
    {
        SessionKey = sessionKey;
    }
}