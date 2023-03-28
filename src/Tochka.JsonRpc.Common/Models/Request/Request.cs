using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

[ExcludeFromCodeCoverage]
public class Request<TParams> : ICall<TParams>
    where TParams : class?
{
    public IRpcId? Id { get; set; }

    public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

    public string Method { get; set; }

    public TParams Params { get; set; }

    public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions)
    {
        var serializedParams = Utils.SerializeParams(Params, serializerOptions);
        return new UntypedRequest
        {
            Id = Id,
            Method = Method,
            Params = serializedParams
        };
    }

    public override string ToString() => $"{nameof(Request<object>)}: {nameof(Method)} [{Method}], {nameof(Id)} [{Id}], {nameof(Params)} [{Params}]";
}
