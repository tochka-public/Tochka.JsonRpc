using Microsoft.Extensions.Logging;
using Moq;
using Tochka.JsonRpc.V1.Client;
using Tochka.JsonRpc.V1.Client.Services;
using Tochka.JsonRpc.V1.Client.Settings;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.Benchmarks;

public class OldJsonRpcClient : JsonRpcClientBase
{
    public OldJsonRpcClient(HttpClient client) : base(client, new SnakeCaseJsonRpcSerializer(), new HeaderJsonRpcSerializer(), new OldJsonRpcClientOptions(), new JsonRpcIdGenerator(Mock.Of<ILogger<JsonRpcIdGenerator>>()), Mock.Of<ILogger<OldJsonRpcClient>>())
    {
    }
}
