using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tochka.JsonRpc.Client.Services;

namespace Tochka.JsonRpc.Client;

/// <summary>
/// Extensions to register JSON-RPC client in DI container
/// </summary>
[PublicAPI]
public static class Extensions
{
    /// <summary>
    /// Register JSON-RPC client as <typeparamref name="TClient"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to</param>
    /// <typeparam name="TClient">Type of JSON-RPC client</typeparam>
    /// <typeparam name="TImplementation">Type of JSON-RPC client implementation</typeparam>
    [ExcludeFromCodeCoverage]
    public static IHttpClientBuilder AddJsonRpcClient<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class, IJsonRpcClient
        where TImplementation : JsonRpcClientBase, TClient =>
        AddJsonRpcClient<TClient, TImplementation>(services, static (_, _) => { });

    /// <summary>
    /// Register JSON-RPC client as <typeparamref name="TClient"/> and configure internal <see cref="HttpClient" />
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to</param>
    /// <param name="configureClient">Delegate used to configure internal <see cref="HttpClient" /></param>
    /// <typeparam name="TClient">Type of JSON-RPC client</typeparam>
    /// <typeparam name="TImplementation">Type of JSON-RPC client implementation</typeparam>
    /// <returns></returns>
    public static IHttpClientBuilder AddJsonRpcClient<TClient, TImplementation>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class, IJsonRpcClient
        where TImplementation : JsonRpcClientBase, TClient
    {
        services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
        var builder = services.AddHttpClient<TClient, TImplementation>(configureClient);
        return builder;
    }

    /// <summary>
    /// Register JSON-RPC client by it's Type
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to</param>
    /// <typeparam name="TClient">Type of JSON-RPC client</typeparam>
    [ExcludeFromCodeCoverage]
    public static IHttpClientBuilder AddJsonRpcClient<TClient>(this IServiceCollection services)
        where TClient : JsonRpcClientBase =>
        AddJsonRpcClient<TClient>(services, static (_, _) => { });

    /// <summary>
    /// Register JSON-RPC client by it's Type and configure internal <see cref="HttpClient" />
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to</param>
    /// <param name="configureClient">Delegate used to configure internal <see cref="HttpClient" /></param>
    /// <typeparam name="TClient">Type of JSON-RPC client</typeparam>
    public static IHttpClientBuilder AddJsonRpcClient<TClient>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where TClient : JsonRpcClientBase
    {
        services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
        var builder = services.AddHttpClient<TClient>(configureClient);
        return builder;
    }
}
