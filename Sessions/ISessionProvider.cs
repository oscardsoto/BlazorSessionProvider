namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Scoped that provide the session for the user
    /// </summary>
    public interface ISessionProvider
    {
        /// <summary>
        /// Service to register any action when the session data has changed
        /// </summary>
        event Action<string, object> SessionDataChanged;

        /// <summary>
        /// Session Id, to access the Keeper
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Creates a new session with a unique Guid
        /// </summary>
        /// <param name="newSession">First Key/Value for the new session to be registered. If so, it will trigger the "OnSessionStart" event (if any)</param>
        void CreateNewSession(KeyValuePair<string, object>? newSession = null);

        /// <summary>
        /// Async method that returns the session value as a <typeparamref name="T"/>. Return the default value if the session don't exist
        /// </summary>
        /// <param name="key">Key for the session</param>
        /// <param name="removeIt">True if the value has to be deleted</param>
        /// <typeparam name="T">Type of object to be returned</typeparam>
        Task<T?> GetSession<T>(string key, bool removeIt = false);

        /// <summary>
        /// Set/Update a value inside the session
        /// </summary>
        /// <param name="key">Identifier for the session value</param>
        /// <param name="value">Value for that id</param>
        void SetSession(string key, object value);

        /// <summary>
        /// Returns true if the key exists in the session
        /// </summary>
        /// <param name="key">Identifier for the session value</param>
        /// <returns></returns>
        Task<bool> ExistKeyInSession(string key);

        /// <summary>
        /// Delete the actual session
        /// </summary>
        void RemoveSession();

        /// <summary>
        /// Async method that returns the Guid if the session exists. If does not exist in the dictionary, return an empty string. If does not exist in "localStorage", return null.
        /// </summary>
        Task<string> HasSession();
    }
}
