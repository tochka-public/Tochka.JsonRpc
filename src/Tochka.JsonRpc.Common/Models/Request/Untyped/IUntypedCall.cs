using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

public interface IUntypedCall : ICall<JsonDocument>
{
    /// <summary>
    /// Set on deserialization. JSON content corresponding to this object
    /// </summary>
    [JsonIgnore]
    string RawJson { get; set; }
}
