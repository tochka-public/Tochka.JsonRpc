using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tochka.JsonRpc.Client.Services;

namespace Tochka.JsonRpc.Client;

[PublicAPI]
public static class Extensions
{
    public static IHttpClientBuilder AddJsonRpcClient<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class, IJsonRpcClient
        where TImplementation : JsonRpcClientBase, TClient =>
        AddJsonRpcClient<TClient, TImplementation>(services, static (_, _) => { });

    public static IHttpClientBuilder AddJsonRpcClient<TClient, TImplementation>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class, IJsonRpcClient
        where TImplementation : JsonRpcClientBase, TClient
    {
        services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
        var builder = services.AddHttpClient<TClient, TImplementation>(configureClient);
        return builder;
    }

    public static IHttpClientBuilder AddJsonRpcClient<TClient>(this IServiceCollection services)
        where TClient : JsonRpcClientBase =>
        AddJsonRpcClient<TClient>(services, static (_, _) => { });

    public static IHttpClientBuilder AddJsonRpcClient<TClient>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : JsonRpcClientBase
    {
        services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
        var builder = services.AddHttpClient<TClient>(configureClient);
        return builder;
    }
}
