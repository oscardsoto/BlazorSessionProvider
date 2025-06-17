using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Scoped that provide the session for the user
    /// </summary>
    public class SessionProvider : ISessionProvider
    {
        private readonly ISessionKeeper _keeper;
        private readonly ISessionBridge _bridge;
        private readonly NavigationManager _navManager;
        private readonly SessionProviderConfig _config;
        private bool _sessionError = false;

        /// <summary>
        /// Session Id, to access the Keeper
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        /// <summary>
        /// Service to register any action when the session data has changed
        /// </summary>
        public event Action<string, object>? SessionDataChanged;

        /// <summary>
        /// Scoped that provide the session for the user
        /// </summary>
        public SessionProvider(ISessionKeeper keeper, ISessionBridge bridge, NavigationManager navigation, IOptions<SessionProviderConfig> options)
        {
            _keeper     = keeper;
            _bridge     = bridge;
            _navManager = navigation;
            _config     = options.Value;
        }

        
        /// <summary>
        /// Creates a new session with a unique Guid
        /// </summary>
        /// <param name="newSession">First Key/Value for the new session to be registered. If so, it will trigger the "OnSessionStart" event (if any)</param>
        public async void CreateNewSession(KeyValuePair<string, object>? newSession = null)
        {
            Id = await _bridge.SetNewSessionId();
            _keeper.AddSession(Id, _config.TimeDelay);

            if (newSession == null)
                _config.OnSessionStart?.Invoke(null);
            else
            {
                _config.OnSessionStart?.Invoke(newSession);
                _keeper.SetSessionInData(Id, newSession.Value.Key, newSession.Value.Value);
            }
        }

        /// <summary>
        /// Async method that returns the Guid if the session exists. If is expired, return an empty string. If does not exist in the bridge or in the keeper, return null.
        /// </summary>
        public async Task<string> HasSession()
        {
            if (string.IsNullOrEmpty(Id))
                Id = await _bridge.GetSessionId();

            if ((Id == null) || !_keeper.HasSessionData(Id))
                return null;

            if (_keeper.IsSessionExpired(Id))
                return "";

            return Id;
        }

        /// <summary>
        /// Async method that returns the session value as a <typeparamref name="T"/>. Return the default value if the session don't exist
        /// </summary>
        /// <param name="key">Key for the session</param>
        /// <param name="removeIt">True if the value has to be deleted</param>
        /// <typeparam name="T">Type of object to be returned</typeparam>
        public async Task<T?> GetSession<T>(string key, bool removeIt = false)
        {
            string id = await HasSession();
            if (id == null)
            {
                if (!_sessionError)
                {
                    _sessionError = true;
                    if (_config.HasNotFoundUrl)
                        _navManager.NavigateTo(_config.SessionNotFoundUrl);
                }
                return default;
            }
            if (id.Equals(string.Empty))
            {
                if (!_sessionError)
                {
                    _sessionError = true;
                    var _sess = _keeper.GetSessionData(await _bridge.GetSessionId());
                    _sess.IgnoreExpired = true;
                    _config.OnSessionExpired?.Invoke(_sess);
                    if (_config.HasExpiredUrl)
                        _navManager.NavigateTo(_config.SessionExpiredUrl);
                }
                return default;
            }

            _sessionError = false;
            var sess = _keeper.GetSessionData(id);
            if (!sess.Exists(key))
                throw new KeyNotFoundException($"The session \"{key}\" does not exist");

            // If the object is null, and -T- is nullable, is fine
            object objInSess = removeIt ? sess.Pull(key) : sess.Get(key);
            if ((objInSess is T) || (objInSess == null && default(T) == null))
                return (T)objInSess;

            // Otherwise, it has to be an error
            throw new InvalidCastException($"Cannot get \"{key}\" correctly. Expected: {typeof(T).Name}. Received: {objInSess?.GetType().Name ?? "Null"}");
        }

        /// <summary>
        /// Delete the actual session
        /// </summary>
        public async void RemoveSession()
        {
            string id = await HasSession();
            if (id == null)
            {
                if (!_sessionError)
                {
                    _sessionError = true;
                    if (_config.HasNotFoundUrl)
                        _navManager.NavigateTo(_config.SessionNotFoundUrl);
                }
                return;
            }
            if (id.Equals(string.Empty))
            {
                if (!_sessionError)
                {
                    _sessionError = true;
                    var _sess = _keeper.GetSessionData(await _bridge.GetSessionId());
                    _sess.IgnoreExpired = true;
                    _config.OnSessionExpired?.Invoke(_sess);
                    if (_config.HasExpiredUrl)
                        _navManager.NavigateTo(_config.SessionExpiredUrl);
                }
                return;
            }

            _sessionError = false;
            _config.OnSessionEnd.Invoke(_keeper.GetSessionData(id));
            _keeper.RemoveSession(id);
            await _bridge.RemoveSessionId();
        }

        /// <summary>
        /// Set/Update a value inside the session
        /// </summary>
        /// <param name="key">Identifier for the session value</param>
        /// <param name="value">Value for that id</param>
        public async void SetSession(string key, object value)
        {
            string id = await HasSession();
            if (id == null)
            {
                if (!_sessionError)
                {
                    _sessionError = true;
                    if (_config.HasNotFoundUrl)
                        _navManager.NavigateTo(_config.SessionNotFoundUrl);
                }
                return;
            }
            if (id.Equals(string.Empty))
            {
                if (!_sessionError)
                {
                    _sessionError = true;
                    var _sess = _keeper.GetSessionData(await _bridge.GetSessionId());
                    _sess.IgnoreExpired = true;
                    _config.OnSessionExpired?.Invoke(_sess);
                    if (_config.HasExpiredUrl)
                        _navManager.NavigateTo(_config.SessionExpiredUrl);
                }
                return;
            }

            _sessionError = false;
            _keeper.SetSessionInData(id, key, value);
            SessionDataChanged?.Invoke(key, value);
        }
    }
}
