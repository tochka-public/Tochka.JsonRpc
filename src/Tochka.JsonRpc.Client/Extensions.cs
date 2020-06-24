using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client
{
    public static class Extensions
    {
        public static IHttpClientBuilder AddJsonRpcClient<TClient, TImplementation>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient=null)
        where TClient : class, IJsonRpcClient
        where TImplementation : JsonRpcClientBase, TClient
        {
            services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
            services.TryAddSingleton<HeaderRpcSerializer>();
            var builder = services.AddHttpClient<TClient, TImplementation>(configureClient ?? ((s, c) => { }));
            return builder;
        }

        public static IHttpClientBuilder AddJsonRpcClient<TClient>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient = null)
            where TClient : JsonRpcClientBase
        {
            services.TryAddSingleton<IJsonRpcIdGenerator, JsonRpcIdGenerator>();
            services.TryAddSingleton<HeaderRpcSerializer>();
            var builder = services.AddHttpClient<TClient>(configureClient ?? ((s, c) => { }));
            return builder;
        }
    }
}
