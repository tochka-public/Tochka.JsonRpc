using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

[ExcludeFromCodeCoverage]
public record Request<TParams>(IRpcId Id, string Method, TParams? Params, string Jsonrpc = JsonRpcConstants.Version) : ICall<TParams>
    where TParams : class
{
    // required for autodoc metadata generation
    internal Request() : this(null!)
    {
    }

    public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions)
    {
        var serializedParams = Utils.SerializeParams(Params, serializerOptions);
        return new UntypedRequest(Id, Method, serializedParams);
    }
}
