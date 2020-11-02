using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;

namespace Tochka.JsonRpc.Common.Models.Response
{
    [ExcludeFromCodeCoverage]
    public class Response<TResult> : IResponse
    {
        public IRpcId Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <example>2.0</example>
        public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

        public TResult Result { get; set; }

        public override string ToString() => $"{nameof(Request<object>)}<{typeof(TResult).Name}>: {nameof(Id)} [{Id}], {nameof(Result)} [{Result}]";
    }
}