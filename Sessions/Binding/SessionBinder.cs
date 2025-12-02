using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BlazorSessionProvider.Sessions.Binding;

/// <summary>
/// Base class for objects whose properties can automatically synchronized with a session when marked with <see cref="BindToKey"/>
/// </summary>
public class SessionBinder : INotifyPropertyChanged
{
    private readonly Dictionary<string, object?> _sessionValues = new();

    internal bool IsInitialized { get; set; }

    /// <summary>
    /// Every time a property is setted, the SessionProvider updates the value in the corresponding key
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Base class for objects whose properties can automatically synchronized with a session when marked with <see cref="BindToKey"/>
    /// </summary>
    public SessionBinder()
    {
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            var bind = prop.GetCustomAttribute<BindToKey>();
            if (bind == null)
                continue;

            var key = string.IsNullOrEmpty(bind.SessionKey) ? prop.Name : bind.SessionKey;
            _sessionValues.Add(key, null);
        }
        IsInitialized = false;
    }

    /// <summary>
    /// Gets the value from hte internal reference. If the class is not binded, this method will return the default value for <typeparamref name="T"/>
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns></returns>
    protected T GetValue<T>([CallerMemberName] string propertyName = "")
    {
        if (!IsInitialized)
            return default!;

        var prop = GetType().GetProperty(propertyName);
        var binding = prop?.GetCustomAttribute<BindToKey>();
        if (binding == null)
            return default!;

        var key = string.IsNullOrEmpty(binding.SessionKey) ? propertyName : binding.SessionKey;
        return _sessionValues.TryGetValue(key, out var value) ? (T)value! : default!;
    }

    /// <summary>
    /// Sets the value to the internal reference, and then triggers the "PropertyChanged" event. If the class is not binded, this method will do nothing
    /// </summary>
    /// <param name="value">Value to set for the reference</param>
    /// <param name="propertyName">Name of the property</param>
    /// <typeparam name="T"></typeparam>
    protected void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
    {
        if (!IsInitialized)
            return;

        var prop = GetType().GetProperty(propertyName);
        var binding = prop?.GetCustomAttribute<BindToKey>();
        if (binding == null)
            return;

        var key = string.IsNullOrEmpty(binding.SessionKey) ? propertyName : binding.SessionKey;
        _sessionValues[key] = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void SetInitialValue(string key, object? value) => _sessionValues[key] = value;
    
    internal object? GetCurrentValue(string key) => _sessionValues.TryGetValue(key, out var value) ? value : null;

    /// <summary>
    /// True if the class is already binded to the session
    /// </summary>
    /// <returns></returns>
    public bool IsBindingInitialized() => IsInitialized;
}