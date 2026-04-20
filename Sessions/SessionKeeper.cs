using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Options;

namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Singleton that keeps all sessions on a ConcurrentDictionary
    /// </summary>
    public class SessionKeeper : ISessionKeeper, IDisposable
    {
        private ConcurrentDictionary<string, SessionState> Sessions { get; set; }

        private ConcurrentDictionary<string, ImmutableArray<Action<string, object>>> Events = new();
        private readonly SessionProviderConfig _config;
        private readonly Timer _cleanupTimer;

        /// <summary>
        /// Singleton that keeps all sessions on a ConcurrentDictionary
        /// </summary>
        public SessionKeeper(IOptions<SessionProviderConfig> options)
        {
            _config = options.Value;
            Sessions = new();

            var interval = _config.CleanupInterval <= TimeSpan.Zero
                ? TimeSpan.FromMinutes(5)
                : _config.CleanupInterval;

            _cleanupTimer = new Timer(CleanupExpiredSessions, null, interval, interval);
        }

        /// <summary>
        /// Adds a new session in the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="delay">Time to expire the session</param>
        public void AddSession(string idSess, TimeSpan delay) => Sessions.TryAdd(idSess, new SessionState(delay));

        /// <summary>
        /// Get the data from the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public SessionState GetSessionData(string idSess)
        {
            SessionState? sesdict;
            Sessions.TryGetValue(idSess, out sesdict);
            TouchSessionIfNeeded(sesdict);
            return sesdict;
        }

        /// <summary>
        /// Deletes the session from the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public void RemoveSession(string idSess)
        {
            Sessions.TryRemove(idSess, out _);
            Events.TryRemove(idSess, out _);
        }

        /// <summary>
        /// Return true if the session exist in the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public bool HasSessionData(string idSess)
        {
            if (!Sessions.TryGetValue(idSess, out var session))
                return false;

            TouchSessionIfNeeded(session);
            return true;
        }

        /// <summary>
        /// Return true if the session has expired
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public bool IsSessionExpired(string idSess)
        {
            SessionState? sesdict;
            if (!Sessions.TryGetValue(idSess, out sesdict) || sesdict is null)
                return true;

            if (!sesdict.HasExpired())
                TouchSessionIfNeeded(sesdict);

            return sesdict.HasExpired();
        }

        /// <summary>
        /// Set a new value inside the session's data
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="key">Identifier for the session value</param>
        /// <param name="value">Value for that id</param>
        public SessionState SetSessionInData(string idSess, string key, object value)
        {
            SessionState? sesdict;
            Sessions.TryGetValue(idSess, out sesdict);
            TouchSessionIfNeeded(sesdict);
            sesdict?.Set(key, value);
            return sesdict;
        }

        /// <summary>
        /// Subscribe a client's event to the keeper in the session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="_event">Event that will receive the key and value</param>
        public void Subscribe(string idSess, Action<string, object> _event)
        {
            Events.AddOrUpdate(
                idSess,
                _ => ImmutableArray.Create(_event),
                (_, current) => current.Contains(_event) ? current : current.Add(_event)
            );
        }

        /// <summary>
        /// Unsubscribe a client's event from the keeper in the session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="_event">Event that will receive the key and value</param>
        public void Unsubscribe(string idSess, Action<string, object> _event)
        {
            while (Events.TryGetValue(idSess, out var handlers))
            {
                var idx = handlers.IndexOf(_event);
                if (idx < 0)
                    return;

                var updated = handlers.RemoveAt(idx);
                if (updated.IsDefaultOrEmpty)
                {
                    if (Events.TryRemove(new KeyValuePair<string, ImmutableArray<Action<string, object>>>(idSess, handlers)))
                        return;
                }
                else if (Events.TryUpdate(idSess, updated, handlers))
                    return;
            }
        }

        /// <summary>
        /// Notify all clients subscribed for a change in the session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="key">Session key</param>
        /// <param name="value">Session value</param>
        public void NotifyAllClients(string idSess, string key, object value)
        {
            if (!Events.TryGetValue(idSess, out var handlers))
                return;

            foreach (var evnt in handlers)
                evnt.Invoke(key, value);
        }

        /// <summary>
        /// Clear all subscriptions for the current session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public void ClearSubscriptions(string idSess) => Events.TryRemove(idSess, out _);

        private void TouchSessionIfNeeded(SessionState? session)
        {
            if (session is null)
                return;

            if (_config.UseRelativeTtl)
                session.RefreshLimit();
        }

        private void CleanupExpiredSessions(object? _)
        {
            foreach (var kvp in Sessions)
            {
                if (!kvp.Value.HasExpired())
                    continue;

                if (!Sessions.TryRemove(new KeyValuePair<string, SessionState>(kvp.Key, kvp.Value)))
                    continue;

                kvp.Value.IgnoreExpired = true;
                _config.TriggerEventExpired(kvp.Value);
                Events.TryRemove(kvp.Key, out _);
            }
        }

        public void Dispose() => _cleanupTimer.Dispose();
    }
}
