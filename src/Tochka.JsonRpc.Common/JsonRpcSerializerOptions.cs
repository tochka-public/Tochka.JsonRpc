using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Converters;

namespace Tochka.JsonRpc.Common;

/// <summary>
/// Predefined <see cref="JsonSerializerOptions" /> for "headers" and data serialization
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static class JsonRpcSerializerOptions
{
    /// <summary>
    /// Options to serialize JSON-RPC "headers": `id`, `jsonrpc`, etc.
    /// </summary>
    /// <remarks>
    /// Changing this not recommended, because request/response "header" object format is fixed and does not imply any changes.
    /// </remarks>
    public static JsonSerializerOptions Headers { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters =
        {
            new RequestWrapperConverter(),
            new CallConverter(),

            new JsonRpcIdConverter(),

            new ResponseWrapperConverter(),
            new ResponseConverter()
        },
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Options for data serialization with snake_case PropertyNamingPolicy and enums conversion
    /// </summary>
    public static JsonSerializerOptions SnakeCase { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        /*
         * Bug: dotnet6 JsonStringEnumConverter ignores policy on deserialization.
         * Fixed in dotnet7 so use JsonStringEnumConverter after upgrading and remove `Macross.Json.Extensions` dependency
         */
        Converters = { new JsonStringEnumMemberConverter(JsonNamingPolicy.SnakeCaseLower) },
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Options for data serialization with camelCase PropertyNamingPolicy and enums conversion
    /// </summary>
    public static JsonSerializerOptions CamelCase { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        /*
         * Bug: dotnet6 JsonStringEnumConverter ignores policy on deserialization.
         * Fixed in dotnet7 so use JsonStringEnumConverter after upgrading and remove `Macross.Json.Extensions` dependency
         */
        Converters = { new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
