using System.ComponentModel;
using System.Reflection;

namespace BlazorSessionProvider.Sessions.Binding;

/// <summary>
/// Service to bind the <typeparamref name="T"/> class to its specified session keys
/// </summary>
public sealed class SessionBindingService<T> : IAsyncDisposable where T : SessionBinder, new()
{
    private readonly ISessionProvider _provider;
    T? Binded { get; set; }

    /// <summary>
    /// Service to bind the <typeparamref name="T"/> class to its specified session keys
    /// </summary>
    /// <param name="provider"></param>
    public SessionBindingService(ISessionProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Returns a <typeparamref name="T"/> class, binded and initialized to the specified session keys. 
    /// If the session has not been created, returns the default value to <typeparamref name="T"/>, without any bind. 
    /// If the <typeparamref name="T"/> instance has already been initialized, returns that instance (does not initialize a new one)
    /// </summary>
    /// <returns></returns>
    public async Task<T> InitializeBindedAsync()
    {
        if (HasBinded())
            return Binded!;

        var sess = await _provider.HasSession();
        if (string.IsNullOrEmpty(sess))
            return new T(); // Session is not created.

        // Subscribe to the keeper
        await _provider.SubscribeToKeeper(OnDataChanged);

        // Suscribe to the Binded's event
        Binded = new T();
        Binded.IsInitialized = true;
        Binded.PropertyChanged += OnPropertyChangedHandler;

        var props = Binded?.GetType().GetProperties() ?? [];
        foreach (var prop in props)
        {
            var bind = prop.GetCustomAttribute<BindToKey>();
            if (bind == null)
                continue;

            var sessBind = string.IsNullOrEmpty(bind.SessionKey) ? prop.Name : bind.SessionKey;
            object? valueSess = null;
            if (await _provider.ExistKeyInSession(sessBind))
                valueSess = await _provider.GetSession<object>(sessBind);

            // Save the data in the internal dictionary
            Binded!.SetInitialValue(sessBind, valueSess);
        }

        return Binded!;
    }

    /// <summary>
    /// Returns true if the <typeparamref name="T"/> object is initialized and binded to the session
    /// </summary>
    /// <returns></returns>
    public bool HasBinded()
    {
        if (Binded == null)
            return false;

        return Binded.IsInitialized;
    }

    private void OnDataChanged(string key, object value)
    {
        if (!HasBinded())
            return;

        var props = Binded?.GetType().GetProperties() ?? [];
        foreach (var prop in props)
        {
            var bind = prop.GetCustomAttribute<BindToKey>();
            if (bind == null)
                continue;

            var sessBind = string.IsNullOrEmpty(bind.SessionKey) ? prop.Name : bind.SessionKey;
            if (sessBind.Equals(key))
            {
                Binded!.SetInitialValue(sessBind, value);
                break;
            }
        }
    }

    private async void OnPropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not SessionBinder binder)
            return;

        var prop = sender.GetType().GetProperty(e.PropertyName!);
        var bind = prop?.GetCustomAttribute<BindToKey>() ?? null;
        if (bind == null)
            return;

        var sessionKey = string.IsNullOrEmpty(bind.SessionKey) ? prop!.Name : bind.SessionKey;
        var value = binder.GetCurrentValue(sessionKey);

        await _provider.SetSession(sessionKey, value!);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_provider != null)
            await _provider.UnsubscribeToKeeper(OnDataChanged);
        
        Binded = null;
    }
}