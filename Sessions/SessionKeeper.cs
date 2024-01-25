using System.Collections.Concurrent;

namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Singleton that keeps all sessions on a ConcurrentDictionary
    /// </summary>
    public class SessionKeeper : ISessionKeeper
    {
        private ConcurrentDictionary<string, SessionState> Sessions { get; set; }

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
        public void SetSessionInData(string idSess, string key, object value)
        {
            SessionState? sesdict;
            Sessions.TryGetValue(idSess, out sesdict);
            sesdict?.Set(key, value);
        }
    }
}
