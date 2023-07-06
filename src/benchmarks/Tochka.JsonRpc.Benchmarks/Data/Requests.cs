using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.Data;

internal static class Requests
{
    public static string GetRequest(Guid id, string method, TestData parameters) => $$"""
        {
            "id": "{{id}}",
            "method": "{{method}}",
            "params": {{JsonSerializer.Serialize(parameters, JsonRpcSerializerOptions.SnakeCase)}},
            "jsonrpc": "2.0"
        }
        """;

    public static string GetNotification(string method, TestData parameters) => $$"""
        {
            "method": "{{method}}",
            "params": {{JsonSerializer.Serialize(parameters, JsonRpcSerializerOptions.SnakeCase)}},
            "jsonrpc": "2.0"
        }
        """;

    public static IEnumerable<TestData> AllDataValues => new[]
    {
        TestData.Big,
        TestData.Nested,
        TestData.Plain
    };
}
