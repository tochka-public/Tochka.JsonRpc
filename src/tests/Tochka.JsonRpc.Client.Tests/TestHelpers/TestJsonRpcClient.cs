using System.Net.Http;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Client.Services;

namespace Tochka.JsonRpc.Client.Tests.TestHelpers;

public class TestJsonRpcClient : JsonRpcClientBase,
    ITestJsonRpcClient
{
    public TestJsonRpcClient(HttpClient client, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log) : base(client, jsonRpcIdGenerator, log)
    {
    }
}
