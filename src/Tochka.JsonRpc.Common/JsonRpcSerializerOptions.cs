using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Converters;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Yoh.Text.Json.NamingPolicies;

namespace Tochka.JsonRpc.Common;

[PublicAPI]
[ExcludeFromCodeCoverage]
public static class JsonRpcSerializerOptions
{
    public static JsonSerializerOptions Headers { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower,
        Converters =
        {
            // TODO: server
            // new RequestWrapperConverter(),
            // new CallConverter(),

            new JsonRpcIdConverter(),

            new ResponseWrapperConverter(),
            new ResponseConverter(),
            new UntypedCallMappingConverter()
        },
        WriteIndented = true
    };

    public static JsonSerializerOptions SnakeCase { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicies.SnakeCaseLower) },
        WriteIndented = true
    };

    public static JsonSerializerOptions CamelCase { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true
    };
}
