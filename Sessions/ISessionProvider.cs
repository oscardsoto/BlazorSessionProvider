namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Scoped that provide the session for the user
    /// </summary>
    public interface ISessionProvider
    {
        /// <summary>
        /// Session Id, to access the Keeper
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Creates a new session with a unique Guid
        /// </summary>
        /// <param name="newSession">First Key/Value for the new session to be registered. If so, it will trigger the "OnSessionStart" event (if any)</param>
        Task CreateNewSession(KeyValuePair<string, object>? newSession = null);

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
        Task SetSession(string key, object value);

        /// <summary>
        /// Returns true if the key exists in the session
        /// </summary>
        /// <param name="key">Identifier for the session value</param>
        /// <returns></returns>
        Task<bool> ExistKeyInSession(string key);

        /// <summary>
        /// Delete the actual session
        /// </summary>
        Task RemoveSession();

        /// <summary>
        /// Async method that returns the Guid if the session exists. If does not exist in the dictionary, return an empty string. If does not exist in cookies, return null.
        /// </summary>
        Task<string> HasSession();

        /// <summary>
        /// Register a function to the keeper to trigger when a sesison value is changed
        /// </summary>
        /// <param name="action">Action object to register</param>
        /// <returns></returns>
        Task SubscribeToKeeper(Action<string, object> action);

        /// <summary>
        /// Removes the function registered from the keeper
        /// </summary>
        /// <param name="action">Action object to remove</param>
        /// <returns></returns>
        Task UnsubscribeToKeeper(Action<string, object> action);
    }
}
