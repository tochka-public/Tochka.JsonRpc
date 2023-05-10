using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Converters;
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
            new RequestWrapperConverter(),
            new CallConverter(),

            new JsonRpcIdConverter(),

            new ResponseWrapperConverter(),
            new ResponseConverter(),
            new UntypedCallMappingConverter()
        },
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static JsonSerializerOptions SnakeCase { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicies.SnakeCaseLower) },
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static JsonSerializerOptions CamelCase { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
