using System.Text.Json;
using Tochka.JsonRpc.Server.Serialization;
using Yoh.Text.Json.NamingPolicies;

namespace Tochka.JsonRpc.Tests.WebApplication;

internal class KebabCaseUpperJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public JsonSerializerOptions Options => new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicies.KebabCaseUpper
    };
}
