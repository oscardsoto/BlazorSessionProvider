using Microsoft.Extensions.DependencyInjection;
using BlazorSessionProvider.Sessions;

namespace BlazorSessionProvider
{
    /// <summary>
    /// 
    /// </summary>
    public static class SessionInit
    {
        /// <summary>
        /// Adds an ISessionProvider to the services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Configuration object</param>
        public static void AddSessionProvider(this IServiceCollection services, Action<SessionProviderConfig>? configuration = null)
        {
            if (configuration == null)
                services.Configure<SessionProviderConfig>(config => { });
            else
                services.Configure<SessionProviderConfig>(configuration);
            services.AddSingleton<ISessionKeeper, SessionKeeper>();
            services.AddScoped<ISessionBridge, SessionBridge>();
            services.AddScoped<ISessionProvider, SessionProvider>();
        }

        /// <summary>
        /// Adds a session provider to the services, with the specified SessionBridge Scoped
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Configuration object</param>
        /// <typeparam name="B">Bridge scoped class</typeparam>
        public static void AddSessionProvider<B>(this IServiceCollection services, Action<SessionProviderConfig>? configuration = null)
        {
            if (configuration == null)
                services.Configure<SessionProviderConfig>(config => { });
            else
                services.Configure<SessionProviderConfig>(configuration);
            services.AddSingleton<ISessionKeeper, SessionKeeper>();

            Type bridgeType = typeof(B);
            Type sessionBdg = typeof(ISessionBridge);
            if (!bridgeType.IsAssignableFrom(sessionBdg))
                throw new ArgumentException($"{bridgeType.Name} does not inherit from {sessionBdg.Name}");

            services.AddScoped(typeof(ISessionBridge), typeof(B));
            services.AddScoped<ISessionProvider, SessionProvider>();
        }
    }
}
