using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.Common.Models.Response.Untyped;

[ExcludeFromCodeCoverage]
public class UntypedResponse : Response<JsonDocument>
{
    /// <summary>
    /// Set on deserialization. JSON content corresponding to result property
    /// </summary>
    [JsonIgnore]
    public string RawResult { get; set; }

    /// <summary>
    /// Set on deserialization. JSON content corresponding to id property
    /// </summary>
    [JsonIgnore]
    public string RawId { get; set; }
}
