using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

public interface ICall
{
    string Jsonrpc { get; }

    string Method { get; }

    IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions);
}

public interface ICall<out TParams> : ICall
    where TParams : class?
{
    TParams? Params { get; }
}
