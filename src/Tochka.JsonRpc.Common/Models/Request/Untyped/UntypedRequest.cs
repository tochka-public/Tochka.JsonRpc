using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

[ExcludeFromCodeCoverage]
public class UntypedRequest : Request<JsonDocument?>, IUntypedCall
{
    /// <inheritdoc />
    [JsonIgnore]
    public string RawJson { get; set; }

    /// <summary>
    /// Set on deserialization. JSON content corresponding to id property
    /// </summary>
    [JsonIgnore]
    public string RawId { get; set; }
}
