using System.Collections.Concurrent;

namespace BlazorSessionProvider.Sessions
{
    /// <summary>
    /// Session info
    /// </summary>
    public class SessionState
    {
        private ConcurrentDictionary<string, object> _table { get; set; }

        /// <summary>
        /// Session begin's time
        /// </summary>
        public DateTime SessionTime { get; set; }

        /// <summary>
        /// Session time to expirate
        /// </summary>
        public DateTime SessionLimit { get; set; }

        /// <summary>
        /// True if the session is ignoring the expiration date
        /// </summary>
        public bool IgnoreExpired { get; set; } = false;

        /// <summary>
        /// Session info
        /// </summary>
        public SessionState(TimeSpan delay)
        {
            _table      = new();
            SessionTime = DateTime.Now;
            SessionLimit = SessionTime.Add(delay);
        }

        /// <summary>
        /// Return true if the session has expired
        /// </summary>
        public bool HasExpired() => SessionLimit < DateTime.Now;

        /// <summary>
        /// Return true if the session has the key's id
        /// </summary>
        /// <param name="key">Session key</param>
        public bool Exists(string key) => _table.ContainsKey(key);

        /// <summary>
        /// Get the session value
        /// </summary>
        /// <param name="key">Session key</param>
        public object Get(string key)
        {
            if (!Exists(key) || (HasExpired() && !IgnoreExpired))
                return null;

            _table.TryGetValue(key, out object obj);
            return obj;
        }

        /// <summary>
        /// Add/Update a value in the session
        /// </summary>
        /// <param name="key">Session key</param>
        /// <param name="value">Session value</param>
        public void Set(string key, object value)
        {
            if (HasExpired() && !IgnoreExpired)
                return;

            if (!Exists(key))
            {
                _table.TryAdd(key, value);
                return;
            }

            _table.TryGetValue(key, out object obj);
            _table.TryUpdate(key, value, obj);
        }

        /// <summary>
        /// Return and delete the value in session
        /// </summary>
        /// <param name="key">Session key</param>
        public object Pull(string key)
        {
            if (!Exists(key) || (HasExpired() && !IgnoreExpired))
                return null;

            _table.TryRemove(key, out object obj);
            return obj;
        }
    }
}
