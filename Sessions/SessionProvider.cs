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
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navManager;
        private readonly SessionProviderConfig _config;
        private bool _sessionError = false;

        /// <summary>
        /// Service to register any action when the session data has changed
        /// </summary>
        public event Action<string, object>? SessionDataChanged;

        /// <summary>
        /// Scoped that provide the session for the user
        /// </summary>
        public SessionProvider(ISessionKeeper keeper, IJSRuntime jsRuntime, NavigationManager navigation, IOptions<SessionProviderConfig> options)
        {
            _keeper     = keeper;
            _jsRuntime  = jsRuntime;
            _navManager = navigation;
            _config     = options.Value;
        }

        private async Task<string> SetNewSessionId()
        {
            string sessionId = Guid.NewGuid().ToString();
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _config.KeyId, sessionId);
            return sessionId;
        }

        private async Task<string> GetSessionId() => await _jsRuntime.InvokeAsync<string>("localStorage.getItem", _config.KeyId);

        private async void RemoveSessionId() => await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", _config.KeyId);

        
        /// <summary>
        /// Creates a new session with a unique Guid
        /// </summary>
        /// <param name="newSession">First Key/Value for the new session to be registered. If so, it will trigger the "OnSessionStart" event (if any)</param>
        public async void CreateNewSession(KeyValuePair<string, object>? newSession = null)
        {
            string sessionId = await SetNewSessionId();
            _keeper.AddSession(sessionId, _config.TimeDelay);

            if (newSession == null)
                _config.OnSessionStart?.Invoke(new());
            else
            {
                _config.OnSessionStart?.Invoke(newSession.Value.Value);
                _keeper.SetSessionInData(sessionId, newSession.Value.Key, newSession.Value.Value);
            }
        }

        /// <summary>
        /// Async method that returns the Guid if the session exists. If does not exist in the dictionary, return an empty string. If does not exist in "localStorage", return null.
        /// </summary>
        public async Task<string> HasSession()
        {
            string id = await GetSessionId();
            if ((id == null) || !_keeper.HasSessionData(id))
                return null;

            if (_keeper.IsSessionExpired(id))
                return "";

            return id;
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
                    var _sess = _keeper.GetSessionData(await GetSessionId());
                    _sess.IgnoreExpired = true;
                    _config.OnSessionExpired?.Invoke(_sess);
                    if (_config.HasExpiredUrl)
                        _navManager.NavigateTo(_config.SessionExpiredUrl);
                }
                return default;
            }

            _sessionError = false;
            var sess = _keeper.GetSessionData(id);
            return removeIt ? (T)sess.Pull(key) : (T)sess.Get(key);
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
                    var _sess = _keeper.GetSessionData(await GetSessionId());
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
            RemoveSessionId();
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
                    var _sess = _keeper.GetSessionData(await GetSessionId());
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
