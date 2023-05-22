using System.Text.Json;
using System.Text.Json.Serialization;
using Yoh.Text.Json.NamingPolicies;

namespace Tochka.JsonRpc.OpenRpc;

public static class OpenRpcConstants
{
    public const string DocumentTemplateParameterName = "documentName";
    public const string DefaultDocumentPath = $"openrpc/{{{DocumentTemplateParameterName}}}.json";
    public const string SpecVersion = "1.2.6";
    public const string DefaultServerName = "JSON-RPC";

    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicies.KebabCaseLower)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
