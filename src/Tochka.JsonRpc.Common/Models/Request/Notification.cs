using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

[ExcludeFromCodeCoverage]
public class Notification<TParams> : ICall<TParams>
    where TParams : class?
{
    public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

    public string Method { get; set; }

    public TParams Params { get; set; }

    public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions)
    {
        var serializedParams = Utils.SerializeParams(Params, serializerOptions);
        return new UntypedNotification
        {
            Method = Method,
            Params = serializedParams
        };
    }

    public override string ToString() => $"{nameof(Request<object>)}<{typeof(TParams).Name}>: {nameof(Method)} [{Method}], {nameof(Params)} [{Params}]";
}
