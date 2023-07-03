using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Client.Services;

namespace Tochka.JsonRpc.Client.Tests.TestHelpers;

internal class TestJsonRpcClient : JsonRpcClientBase, ITestJsonRpcClient
{
    public TestJsonRpcClient(HttpClient client, IOptions<TestJsonRpcClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log) : base(client, options.Value, jsonRpcIdGenerator, log)
    {
    }
}
