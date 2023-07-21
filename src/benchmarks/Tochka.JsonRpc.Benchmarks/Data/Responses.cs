using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.Data;

internal static class Responses
{
    public static string GetResponse(Guid id, TestData result) => $$"""
        {
            "id": "{{id}}",
            "result": {{JsonSerializer.Serialize(result, JsonRpcSerializerOptions.SnakeCase)}},
            "jsonrpc": "2.0"
        }
        """;

    public const string PlainKey = "plain";
    public const string NestedKey = "nested";
    public const string BigKey = "big";

    public static IEnumerable<string> AllKeys => new[]
    {
        PlainKey,
        NestedKey,
        BigKey
    };
}
