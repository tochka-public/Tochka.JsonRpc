using System.Collections.Generic;
using System.Net.Http;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models
{
    public interface IJsonRpcCallContext
    {
        IUntypedCall SingleCall { get; }
        List<IUntypedCall> BatchCall { get; }
        int ExpectedBatchResponseCount { get; }
        string HttpResponseInfo { get; }
        string HttpContentInfo { get; }
        string ErrorInfo { get; }
        IError Error { get;  }
        IResponse SingleResponse { get; }
        List<IResponse> BatchResponse { get; }
        void WithRequestUrl(string requestUrl);
        void WithSingle(IUntypedCall singleCall);
        void WithBatch(List<IUntypedCall> batchCall);
        void WithHttpResponse(HttpResponseMessage httpResponseMessage);
        void WithHttpContent(HttpContent httpContent, string httpContentString);
        void WithSingleResponse(IResponse singleResponse);
        void WithBatchResponse(List<IResponse> batchResponse);
        void WithError(UntypedErrorResponse untypedErrorResponse);
        string ToString();
    }
}