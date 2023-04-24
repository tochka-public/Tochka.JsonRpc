using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.DependencyInjection;
using Tochka.JsonRpc.Server.Filters;
using Tochka.JsonRpc.Server.Routing;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddJsonRpcServer(this IServiceCollection services, Action<JsonRpcServerOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.TryAddConvention<JsonRpcActionModelConvention>();
        services.TryAddConvention<JsonRpcParameterModelConvention>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, JsonRpcMatcherPolicy>());
        services.Configure<MvcOptions>(static options =>
        {
            options.OutputFormatters.Insert(0, new SystemTextJsonOutputFormatter(JsonRpcSerializerOptions.Headers));
            options.Filters.Add<JsonRpcActionFilter>(int.MaxValue);
            options.Filters.Add<JsonRpcExceptionFilter>(int.MaxValue);
            options.Filters.Add<JsonRpcResultFilter>(int.MaxValue);
        });
        services.AddSingleton<IJsonRpcErrorFactory, JsonRpcErrorFactory>();
        services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();
        services.AddSingleton<IJsonSerializerOptionsProvider, CamelCaseJsonSerializerOptionsProvider>();
        services.AddSingleton<IJsonRpcParamsParser, JsonRpcParamsParser>();
        services.AddSingleton<IParameterBinder, ParameterBinder>();
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
        serviceCollection.TryAddEnumerable(new ServiceDescriptor(typeof(IConfigureOptions<MvcOptions>), typeof(ModelConventionConfigurator<T>), ServiceLifetime.Singleton));
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
