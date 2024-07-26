using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Client.Tests.Integration;

public class SnakeCaseJsonRpcClient : JsonRpcClientBase
{
    public override JsonSerializerOptions DataJsonSerializerOptions => JsonRpcSerializerOptions.SnakeCase;

    public SnakeCaseJsonRpcClient(HttpClient client, IJsonRpcIdGenerator jsonRpcIdGenerator) : base(client, jsonRpcIdGenerator, Mock.Of<ILogger>())
    {
        client.BaseAddress = new Uri("https://localhost/");
        client.Timeout = TimeSpan.FromSeconds(10);
    }
}
