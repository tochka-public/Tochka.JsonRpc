using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

public interface IUntypedCall : ICall<JsonDocument>
{
}
