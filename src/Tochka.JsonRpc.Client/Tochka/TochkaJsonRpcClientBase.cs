using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tochka
{
    /// <summary>
    /// Base json-rpc client with authentication configuration
    /// </summary>
    public abstract class TochkaJsonRpcClientBase : JsonRpcClientBase
    {
        protected TochkaJsonRpcClientBase(HttpClient client, IJsonRpcSerializer serializer, HeaderJsonRpcSerializer headerJsonRpcSerializer, IOptions<TochkaJsonRpcClientOptionsBase> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log) : base(client, serializer, headerJsonRpcSerializer, options.Value, jsonRpcIdGenerator, log)
        {
            if (string.IsNullOrEmpty(options.Value.AuthenticationKey))
            {
                throw new ArgumentException("Authentication key is empty. You need to specify this in your config");
            }

            client.DefaultRequestHeaders.Add(options.Value.AuthenticationHeader, options.Value.AuthenticationKey);
        }
    }
}
