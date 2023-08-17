using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.OpenRpc.Converters;
using Yoh.Text.Json.NamingPolicies;

namespace Tochka.JsonRpc.OpenRpc;

/// <summary>
/// All OpenRPC constants
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static class OpenRpcConstants
{
    /// <summary>
    /// Template parameter name for OpenRPC document name
    /// </summary>
    public const string DocumentTemplateParameterName = "documentName";

    /// <summary>
    /// Default route to OpenRPC document
    /// </summary>
    public const string DefaultDocumentPath = $"openrpc/{{{DocumentTemplateParameterName}}}.json";

    /// <summary>
    /// OpenRPC specification version
    /// </summary>
    public const string SpecVersion = "1.2.6";

    /// <summary>
    /// Default server name for OpenRPC document
    /// </summary>
    public const string DefaultServerName = "JSON-RPC";

    /// <summary>
    /// <see cref="JsonSerializerOptions" /> for OpenRPC document serialization
    /// </summary>
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicies.KebabCaseLower),
            new UriConverter()
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
