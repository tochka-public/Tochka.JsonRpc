using System.Text.Json;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Client.Tests.WebApplication;

internal class SimpleResponseProvider : IResponseProvider
{
    public JsonDocument GetResponse() => JsonDocument.Parse("{ \"Hello\": \"World!\" }");
    public TestData GetJsonRpcResponse() => TestData.Plain;
}
