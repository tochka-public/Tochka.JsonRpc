using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Tochka;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tests.TochkaJsonRpcClientBase;

public class TochkaTestClient : Tochka.TochkaJsonRpcClientBase
{
    public TochkaTestClient(HttpClient client, IJsonRpcSerializer serializer, HeaderJsonRpcSerializer headerJsonRpcSerializer, IOptions<TochkaTestClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<TochkaTestClient> log) : base(client, serializer, headerJsonRpcSerializer, options, jsonRpcIdGenerator, log)
    {
    }
}
