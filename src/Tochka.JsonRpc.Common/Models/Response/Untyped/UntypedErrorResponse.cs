using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.Common.Models.Response.Untyped;

[ExcludeFromCodeCoverage]
public class UntypedErrorResponse : ErrorResponse<JsonDocument>
{
    /// <summary>
    /// Set on deserialization. JSON content corresponding to error property
    /// </summary>
    [JsonIgnore]
    public string RawError { get; set; }

    /// <summary>
    /// Set on deserialization. JSON content corresponding to id property
    /// </summary>
    [JsonIgnore]
    public string RawId { get; set; }
}
