using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tochka.JsonRpc.V1.Client.Services;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Client
{
    public static class Extensions
    {
        public static IHttpClientBuilder AddJsonRpcClient<TClient, TImplementation>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient=null)
        where TClient : class, IJsonRpcClient
        where TImplementation : JsonRpcClientBase, TClient
        {
            services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
            services.TryAddSingleton<HeaderJsonRpcSerializer>();
            var builder = services.AddHttpClient<TClient, TImplementation>(configureClient ?? ((s, c) => { }));
            return builder;
        }

        public static IHttpClientBuilder AddJsonRpcClient<TClient>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient = null)
            where TClient : JsonRpcClientBase
        {
            services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
            services.TryAddSingleton<HeaderJsonRpcSerializer>();
            var builder = services.AddHttpClient<TClient>(configureClient ?? ((s, c) => { }));
            return builder;
        }
    }
}
