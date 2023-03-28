using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

public interface ICall
{
    string Jsonrpc { get; set; }

    string Method { get; set; }

    IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions);
}

public interface ICall<TParams> : ICall
    where TParams : class?
{
    TParams Params { get; set; }
}
