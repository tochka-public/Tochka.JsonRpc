using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Common.Models.Request
{
    public interface ICall
    {
        string Jsonrpc { get; set; }

        string Method { get; set; }

        IUntypedCall WithSerializedParams(IRpcSerializer serializer);
    }

    public interface ICall<TParams> : ICall
    {
        TParams Params { get; set; }
    }
}