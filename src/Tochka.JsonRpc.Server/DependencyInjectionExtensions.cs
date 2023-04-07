using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Tochka.JsonRpc.Server;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddJsonRpcServer(this IServiceCollection services, Action<JsonRpcServerOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.TryAddConvention<JsonRpcActionModelConvention>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, JsonRpcMatcherPolicy>());
        services.AddSingleton<JsonRpcMarkerService>();
        return services;
    }

    public static IServiceCollection AddJsonRpcServer(this IServiceCollection services) => services.AddJsonRpcServer(static _ => { });

    public static IApplicationBuilder UseJsonRpc(this IApplicationBuilder app)
    {
        EnsureRequiredServicesRegistered(app.ApplicationServices);
        // Unfortunately there is no good way to check if UseRouting wasn't called before it
        return app.UseMiddleware<JsonRpcMiddleware>();
    }

    private static IServiceCollection TryAddConvention<T>(this IServiceCollection serviceCollection)
        where T : class
    {
        serviceCollection.TryAddSingleton<T>();
        serviceCollection.TryAddEnumerable(new ServiceDescriptor(typeof(IConfigureOptions<MvcOptions>), typeof(ConventionConfigurator<T>), ServiceLifetime.Singleton));
        return serviceCollection;
    }

    private static void EnsureRequiredServicesRegistered(IServiceProvider services)
    {
        if (services.GetService<JsonRpcMarkerService>() == null)
        {
            throw new InvalidOperationException();
        }
    }
}
