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
    List<IUntypedCall>? BatchCall { get; }
    int ExpectedBatchResponseCount { get; }
    string? HttpResponseInfo { get; }
    string? HttpContentInfo { get; }
    IError? Error { get; }
    IResponse? SingleResponse { get; }
    List<IResponse>? BatchResponse { get; }

    void WithRequestUrl(string? requestUrl);
    void WithSingle(IUntypedCall singleCall);
    void WithBatch(List<IUntypedCall> batchCall);
    void WithHttpResponse(HttpResponseMessage httpResponseMessage);
    void WithHttpContent(HttpContent httpContent, string httpContentString);
    void WithSingleResponse(IResponse singleResponse);
    void WithBatchResponse(List<IResponse> batchResponse);
    void WithError(UntypedErrorResponse untypedErrorResponse);

    [UsedImplicitly]
    string ToString();
}
