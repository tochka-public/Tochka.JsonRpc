using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Common.Models.Request
{
    public interface ICall
    {
        string Jsonrpc { get; set; }

        string Method { get; set; }

        IUntypedCall WithSerializedParams(IJsonRpcSerializer serializer);
    }

    public interface ICall<TParams> : ICall
    {
        TParams Params { get; set; }
    }
}
