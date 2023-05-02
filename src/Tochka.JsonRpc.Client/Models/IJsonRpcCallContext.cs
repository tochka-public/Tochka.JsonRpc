using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

[PublicAPI]
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
public interface IJsonRpcCallContext
{
    string? RequestUrl { get; }
    IUntypedCall? SingleCall { get; }
    ICollection<IUntypedCall>? BatchCall { get; }
    int ExpectedBatchResponseCount { get; }
    string? HttpResponseInfo { get; }
    string? HttpContentInfo { get; }
    IError? Error { get; }
    IResponse? SingleResponse { get; }
    List<IResponse>? BatchResponse { get; }

    IJsonRpcCallContext WithRequestUrl(string? requestUrl);
    IJsonRpcCallContext WithSingle(IUntypedCall singleCall);
    IJsonRpcCallContext WithBatch(ICollection<IUntypedCall> batchCall);
    IJsonRpcCallContext WithHttpResponse(HttpResponseMessage httpResponseMessage);
    IJsonRpcCallContext WithHttpContent(HttpContent httpContent, string httpContentString);
    IJsonRpcCallContext WithSingleResponse(IResponse singleResponse);
    IJsonRpcCallContext WithBatchResponse(List<IResponse> batchResponse);
    IJsonRpcCallContext WithError(UntypedErrorResponse untypedErrorResponse);

    [UsedImplicitly]
    string ToString();
}
