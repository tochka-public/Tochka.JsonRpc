using System.Text.Json;
using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.Tests.WebApplication;

internal class KebabCaseUpperJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public JsonSerializerOptions Options => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper
    };
}
