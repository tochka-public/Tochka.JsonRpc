using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.V1.Common.Models.Id;
using Tochka.JsonRpc.V1.Common.Models.Request;

namespace Tochka.JsonRpc.V1.Common.Models.Response
{
    [ExcludeFromCodeCoverage]
    public class Response<TResult> : IResponse
    {
        public IRpcId Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

        public TResult Result { get; set; }

        public override string ToString() => $"{nameof(Request<object>)}<{typeof(TResult).Name}>: {nameof(Id)} [{Id}], {nameof(Result)} [{Result}]";
    }
}
