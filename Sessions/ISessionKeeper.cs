namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Singleton that keeps all sessions on a ConcurrentDictionary
    /// </summary>
    public interface ISessionKeeper
    {
        /// <summary>
        /// Adds a new session in the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="delay">Time to expire the session</param>
        void AddSession(string idSess, TimeSpan delay);

        /// <summary>
        /// Deletes the session from the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        void RemoveSession(string idSess);

        /// <summary>
        /// Get the data from the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        SessionState GetSessionData(string idSess);

        /// <summary>
        /// Return true if the session exist in the dictionary
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        bool HasSessionData(string idSess);

        /// <summary>
        /// Set a new value inside the session's data
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="key">Identifier for the session value</param>
        /// <param name="value">Value for that id</param>
        SessionState SetSessionInData(string idSess, string key, object value);

        /// <summary>
        /// Return true if the session has expired
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        bool IsSessionExpired(string idSess);

        /// <summary>
        /// Subscribe a client's event to the keeper
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="_event">Event that will receive the key and value</param>
        void Subscribe(string idSess, Action<string, object> _event);

        /// <summary>
        /// Unsubscribe a client's event from the keeper
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="_event">Event that will receive the key and value</param>
        void Unsubscribe(string idSess, Action<string, object> _event);

        /// <summary>
        /// Notify all clients subscribed for a change in the session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        /// <param name="key">Session key</param>
        /// <param name="value">Session value</param>
        void NotifyAllClients(string idSess, string key, object value);

        /// <summary>
        /// Clear all subscriptions for the current session
        /// </summary>
        /// <param name="idSess">Guid for the session</param>
        void ClearSubscriptions(string idSess);
    }
}
