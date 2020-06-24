using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tests
{
    public class TestClient : JsonRpcClientBase, ITestClient
    {
        public TestClient(HttpClient client, IRpcSerializer serializer, HeaderRpcSerializer headerRpcSerializer, IOptions<TestOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log) : base(client, serializer, headerRpcSerializer, options.Value, jsonRpcIdGenerator, log)
        {
        }
    }
}