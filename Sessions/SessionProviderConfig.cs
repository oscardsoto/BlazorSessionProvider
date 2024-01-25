namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Configuration for Session Provider.
    /// </summary>
    public class SessionProviderConfig
    {
        /// <summary>
        /// Time for the session to expire
        /// </summary>
        public TimeSpan TimeDelay { get; set; } = new(2, 0, 0);

        /// <summary>
        /// Key value in localStorage for the session Identifier
        /// </summary>
        public string KeyId { get; set; } = "blky";

        /// <summary>
        /// Url to navigate when a session is not found
        /// </summary>
        public string SessionNotFoundUrl { get; set; } = "";

        /// <summary>
        /// True if the "SessionNotFoundUrl" has an Url
        /// </summary>
        public bool HasNotFoundUrl
        {
            get
            {
                return !string.IsNullOrEmpty(SessionNotFoundUrl);
            }
        }

        /// <summary>
        /// Url to navigate when a session is expired
        /// </summary>
        public string SessionExpiredUrl { get; set; } = "";

        /// <summary>
        /// True if "SessionExpiredUrl" has an Url
        /// </summary>
        public bool HasExpiredUrl
        {
            get
            {
                return !string.IsNullOrEmpty(SessionExpiredUrl);
            }
        }

        /// <summary>
        /// Event when the session has started
        /// </summary>
        /// <returns></returns>
        public Action<object> OnSessionStart { get; set; } = (value) => {};

        /// <summary>
        /// Event when the session has ended
        /// </summary>
        /// <returns></returns>
        public Action<SessionState> OnSessionEnd { get; set; } = (value) => {};

        /// <summary>
        /// Event when the session has expired
        /// </summary>
        /// <returns></returns>
        public Action<SessionState> OnSessionExpired { get; set; } = (value) => {};

        /// <summary>
        /// Configuration for Session Provider.
        /// </summary>
        public SessionProviderConfig() { }
    }
}
