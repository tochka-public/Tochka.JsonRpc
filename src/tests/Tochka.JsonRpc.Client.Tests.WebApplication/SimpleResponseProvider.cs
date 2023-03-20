using System.Text.Json;

namespace Tochka.JsonRpc.Client.Tests.WebApplication;

internal class SimpleResponseProvider : IResponseProvider
{
    public JsonDocument GetResponse() => JsonDocument.Parse("{ \"Hello\": \"World!\" }");
}
