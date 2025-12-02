using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Timers;

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

        /// <inheritdoc/>
        public string Id { get; set; } = null;

        /// <summary>
        /// Scoped that provide the session for the user
        /// </summary>
        /// <param name="bridge"></param>
        /// <param name="keeper"></param>
        /// <param name="navigation"></param>
        /// <param name="options"></param>
        public SessionProvider(ISessionKeeper keeper, ISessionBridge bridge, NavigationManager navigation, IOptions<SessionProviderConfig> options)
        {
            _keeper = keeper;
            _bridge = bridge;
            _navManager = navigation;
            _config = options.Value;

            if (_config.SyncProviderOnStart || _config.UseHttpOnlyCookies)
                _ = HasSession();
        }

        /// <inheritdoc/>
        public async Task SubscribeToKeeper(Action<string, object> action)
        {
            string id = await HasSession();
            if (string.IsNullOrEmpty(id))
                return;

            _keeper.Subscribe(id, action);
        }

        /// <inheritdoc/>
        public async Task UnsubscribeToKeeper(Action<string, object> action)
        {
            string id = await HasSession();
            if (string.IsNullOrEmpty(id))
                return;

            _keeper.Unsubscribe(id, action);
        }
        
        /// <inheritdoc/>
        public async Task CreateNewSession(KeyValuePair<string, object>? newSession = null)
        {
            Id = await _bridge.SetNewSessionId();
            _keeper.AddSession(Id, _config.TimeDelay);

            if (newSession == null)
            {
                _config.TriggerEventStart(new(TimeSpan.FromSeconds(1)));
                return;
            }
            var data = _keeper.SetSessionInData(Id, newSession!.Value.Key, newSession.Value.Value);
            _config.TriggerEventStart(data);
        }

        /// <inheritdoc/>
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

        async Task<string> ValidateIdReference()
        {
            string id = await HasSession();

            if (string.IsNullOrEmpty(id))
                if (!_sessionError)
                {
                    _sessionError = true;
                    if (id is null && _config.HasNotFoundUrl)
                        _navManager.NavigateTo(_config.SessionNotFoundUrl);
                        
                    else if (id == string.Empty)
                    {
                        var session = _keeper.GetSessionData(await _bridge.GetSessionId());
                        session.IgnoreExpired = true;
                        _config.TriggerEventExpired(session);

                        if (_config.HasExpiredUrl)
                            _navManager.NavigateTo(_config.SessionExpiredUrl);
                    }
                }

            return id;
        }

        /// <inheritdoc/>
        public async Task<T?> GetSession<T>(string key, bool removeIt = false)
        {
            string id = await ValidateIdReference();
            if (string.IsNullOrEmpty(id))
                return default;

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

        /// <inheritdoc/>
        public async Task RemoveSession()
        {
            string id = await ValidateIdReference();
            if (string.IsNullOrEmpty(id))
                return;

            _sessionError = false;
            _config.TriggerEventEnd(_keeper.GetSessionData(id));
            _keeper.RemoveSession(id);
            await _bridge.RemoveSessionId();
        }

        /// <inheritdoc/>
        public async Task SetSession(string key, object value)
        {
            string id = await ValidateIdReference();
            if (string.IsNullOrEmpty(id))
                return;

            _sessionError = false;
            _keeper.SetSessionInData(id, key, value);
            _keeper.NotifyAllClients(id, key, value);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistKeyInSession(string key)
        {
            string id = await ValidateIdReference();
            if (string.IsNullOrEmpty(id))
                return false;

            _sessionError = false;
            var sess = _keeper.GetSessionData(id);
            return sess.Exists(key);
        }
    }
}
