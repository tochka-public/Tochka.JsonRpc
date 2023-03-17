using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.V1.Client;
using Tochka.JsonRpc.V1.Client.Services;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Client.Tests
{
    public class TestClient : JsonRpcClientBase, ITestClient
    {
        public TestClient(HttpClient client, IJsonRpcSerializer serializer, HeaderJsonRpcSerializer headerJsonRpcSerializer, IOptions<TestOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log) : base(client, serializer, headerJsonRpcSerializer, options.Value, jsonRpcIdGenerator, log)
        {
        }
    }
}
