using System.Text.Json;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Tests.WebApplication;

internal class SimpleResponseProvider : IResponseProvider
{
    public JsonDocument GetResponse() => JsonDocument.Parse("{ \"Hello\": \"World!\" }");
    public TestData GetJsonRpcResponse() => TestData.Plain;
}
