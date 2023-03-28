using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tochka.JsonRpc.V1.Client;
using Tochka.JsonRpc.V1.Client.Services;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tests.Integration;

internal class SnakeCaseJsonRpcClient : JsonRpcClientBase
{
    public SnakeCaseJsonRpcClient(HttpClient client, SnakeCaseJsonRpcSerializer serializer, HeaderJsonRpcSerializer headerJsonRpcSerializer, IJsonRpcIdGenerator jsonRpcIdGenerator) : base(client, serializer, headerJsonRpcSerializer, new SimpleJsonRpcClientOptions(), jsonRpcIdGenerator, Mock.Of<ILogger>())
    {
    }
}
