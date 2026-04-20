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
        /// Key value in cookies for the session Identifier
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
        /// True if the SessionProvider retrieves the session ID from the SessionBridge when starting the service (false by default). Set this value to false if your SessionBridge does not manage the session ID at the beginning of the application.
        /// </summary>
        /// <value></value>
        public bool SyncProviderOnStart { get; set; } = false;

        /// <summary>
        /// True to make the SessionBridge use HttpOnly cookies (false by default). If true, make sure to call the SessionBridge methods when IHttpContext is avaiable.
        /// </summary>
        /// <value></value>
        public bool UseHttpOnlyCookies { get; set; } = false;

        /// <summary>
        /// Event when the session has started
        /// </summary>
        /// <returns></returns>
        public event Action<SessionState>? OnSessionStart;

        /// <summary>
        /// Event when the session has ended
        /// </summary>
        /// <returns></returns>
        public event Action<SessionState>? OnSessionEnd;

        /// <summary>
        /// Event when the session has expired
        /// </summary>
        /// <returns></returns>
        public event Action<SessionState>? OnSessionExpired;

        /// <summary>
        /// Interval used by the background cleanup process that removes expired sessions.
        /// </summary>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// True to use a relative TTL (sliding expiration). When enabled, every valid session access refreshes the expiration limit.
        /// False to use an absolute TTL (fixed expiration from creation time).
        /// </summary>
        public bool UseRelativeTtl { get; set; } = false;

        internal void TriggerEventStart(SessionState sessionStarted) => OnSessionStart?.Invoke(sessionStarted);

        internal void TriggerEventEnd(SessionState sessionEnded) => OnSessionEnd?.Invoke(sessionEnded);

        internal void TriggerEventExpired(SessionState sessionExpired) => OnSessionExpired?.Invoke(sessionExpired);

        /// <summary>
        /// Configuration for Session Provider.
        /// </summary>
        public SessionProviderConfig() { }
    }
}
