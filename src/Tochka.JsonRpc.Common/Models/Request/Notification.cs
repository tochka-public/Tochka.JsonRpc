using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

[ExcludeFromCodeCoverage]
public record Notification<TParams>(string Method, TParams? Params, string Jsonrpc = JsonRpcConstants.Version) : ICall<TParams>
    where TParams : class
{
    public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions)
    {
        var serializedParams = Utils.SerializeParams(Params, serializerOptions);
        return new UntypedNotification(Method, serializedParams);
    }
}
