using System.Collections.Concurrent;

namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Singleton that keeps all sessions on a ConcurrentDictionary
    /// </summary>
    public class SessionKeeper : ISessionKeeper
    {
        private ConcurrentDictionary<string, SessionState> Sessions { get; set; }

        private ConcurrentDictionary<string, List<Action<string, object>>> Events = new();

        /// <summary>
        /// Singleton that keeps all sessions on a ConcurrentDictionary
        /// </summary>
        public SessionKeeper() => Sessions = new();

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
            return sesdict;
        }

        /// <summary>
        /// Deletes the session from the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public void RemoveSession(string idSess) => Sessions.TryRemove(idSess, out _);

        /// <summary>
        /// Return true if the session exist in the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public bool HasSessionData(string idSess) => Sessions.TryGetValue(idSess, out _);

        /// <summary>
        /// Return true if the session has expired
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public bool IsSessionExpired(string idSess)
        {
            SessionState? sesdict;
            Sessions.TryGetValue(idSess, out sesdict);
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
            if (!Events.ContainsKey(idSess))
            {
                Events.TryAdd(idSess, new List<Action<string, object>> { _event });
                return;
            }

            List<Action<string, object>>? events;
            Events.TryGetValue(idSess, out events);
            events?.Add(_event);
        }

        /// <summary>
        /// Unsubscribe a client's event from the keeper in the session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="_event">Event that will receive the key and value</param>
        public void Unsubscribe(string idSess, Action<string, object> _event)
        {
            if (!Events.ContainsKey(idSess))
                return;

            List<Action<string, object>>? events;
            Events.TryGetValue(idSess, out events);
            if ((events != null) && events.Contains(_event))
                events.Remove(_event);
        }

        /// <summary>
        /// Notify all clients subscribed for a change in the session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="key">Session key</param>
        /// <param name="value">Session value</param>
        public void NotifyAllClients(string idSess, string key, object value)
        {
            if (!Events.ContainsKey(idSess))
                return;

            List<Action<string, object>>? events;
            Events.TryGetValue(idSess, out events);
            if (events != null)
                foreach (var evnt in events)
                    evnt.Invoke(key, value);
        }

        /// <summary>
        /// Clear all subscriptions for the current session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        public void ClearSubscriptions(string idSess) => Events.TryRemove(idSess, out _);
    }
}
