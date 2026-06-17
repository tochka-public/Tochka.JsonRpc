using Asp.Versioning.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tochka.JsonRpc.ApiExplorer;

/// <summary>
/// Extensions to configure using Swagger for JSON-RPC in application
/// </summary>
public static class Extensions
{
    public static IServiceCollection AddJsonRpcOpenApiIntegration(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiControllerSpecification, JsonRpcControllerSpecification>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, JsonRpcDescriptionProvider>());
        services.TryAddSingleton<ITypeEmitter, TypeEmitter>();
        return services;
    }
}
