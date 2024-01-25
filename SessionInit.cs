using Microsoft.Extensions.DependencyInjection;
using BlazorSessionProvider.Sessions;

namespace BlazorSessionProvider
{
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
            services.AddScoped<ISessionProvider, SessionProvider>();
        }
    }
}
