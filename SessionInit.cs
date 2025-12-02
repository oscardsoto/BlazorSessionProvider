using Microsoft.Extensions.DependencyInjection;
using BlazorSessionProvider.Sessions;
using BlazorSessionProvider.Sessions.Binding;

namespace BlazorSessionProvider
{
    /// <summary>
    /// 
    /// </summary>
    public static class SessionInit
    {
        /// <summary>
        /// Adds an ISessionProvider to the services. This method implements a HttpContextAccessor in the service collection for the SessionBridge.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Configuration object</param>
        public static void AddSessionProvider(this IServiceCollection services, Action<SessionProviderConfig>? configuration = null)
        {
            if (services.Any(srv => srv.ServiceType == typeof(ISessionKeeper)))
                return;

            services.Configure<SessionProviderConfig>(configuration == null ? config => { } : configuration);
            services.AddSingleton<ISessionKeeper, SessionKeeper>();
            services.AddHttpContextAccessor();
            services.AddScoped<ISessionBridge, SessionBridge>();
            services.AddScoped<ISessionProvider, SessionProvider>();
        }

        /// <summary>
        /// Adds a session provider to the services, with the specified SessionBridge Scoped. This method does not implements a HttpContextAccessor.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Configuration object</param>
        /// <typeparam name="B">Bridge scoped class</typeparam>
        public static void AddSessionProvider<B>(this IServiceCollection services, Action<SessionProviderConfig>? configuration = null) where B : ISessionBridge
        {
            if (services.Any(srv => srv.ServiceType == typeof(ISessionKeeper)))
                return;
                
            services.Configure<SessionProviderConfig>(configuration == null ? config => { } : configuration);
            services.AddSingleton<ISessionKeeper, SessionKeeper>();
            services.AddScoped(typeof(ISessionBridge), typeof(B));
            services.AddScoped<ISessionProvider, SessionProvider>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="B"></typeparam>
        /// <param name="services"></param>
        public static void AddBindingService<B>(this IServiceCollection services) where B : SessionBinder, new()
        {
            if (!services.Any(srv => srv.ServiceType == typeof(SessionBindingService<B>)))
                services.AddScoped<SessionBindingService<B>>();
        }
    }
}
