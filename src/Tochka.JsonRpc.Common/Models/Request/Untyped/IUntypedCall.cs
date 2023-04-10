using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

public interface IUntypedCall : ICall<JsonDocument>
{
}
