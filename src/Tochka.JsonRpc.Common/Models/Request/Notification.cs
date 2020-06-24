using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Common.Models.Request
{
    [ExcludeFromCodeCoverage]
    public class Notification<TParams> : ICall<TParams>
    {
        public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

        public string Method { get; set; }

        public TParams Params { get; set; }

        public IUntypedCall WithSerializedParams(IRpcSerializer serializer)
        {
            var serializedParams = serializer.SerializeParams(Params);
            return new UntypedNotification()
            {
                Method = Method,
                Params = serializedParams
            };
        }

        public override string ToString() => $"{nameof(Request<object>)}<{typeof(TParams).Name}>: {nameof(Method)} [{Method}], {nameof(Params)} [{Params}]";
    }
}