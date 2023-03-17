using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.V1.Common.Models.Id;
using Tochka.JsonRpc.V1.Common.Models.Request;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.V1.Common.Models.Response
{
    [ExcludeFromCodeCoverage]
    public class ErrorResponse<TError> : IErrorResponse
    {
        public IRpcId Id { get; set; }

        public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

        public Error<TError> Error { get; set; }

        public override string ToString() => $"{nameof(Request<object>)}<{typeof(TError).Name}>: {nameof(Id)} [{Id}], {nameof(Error)} [{Error}]";
    }
}
