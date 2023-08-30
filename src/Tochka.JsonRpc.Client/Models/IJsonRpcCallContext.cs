using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

/// <summary>
/// Context with all information about request and response
/// </summary>
[PublicAPI]
public interface IJsonRpcCallContext
{
    /// <summary>
    /// Additional request URL to send HTTP request to
    /// </summary>
    string? RequestUrl { get; }

    /// <summary>
    /// Call if it was request or notification
    /// </summary>
    IUntypedCall? SingleCall { get; }

    /// <summary>
    /// Collection of calls if it was batch
    /// </summary>
    ICollection<IUntypedCall>? BatchCall { get; }

    /// <summary>
    /// Number of responses that should return in response of batch request
    /// </summary>
    int ExpectedBatchResponseCount { get; }

    /// <summary>
    /// Serialized HttpResponseMessage
    /// </summary>
    string? HttpResponseInfo { get; }

    /// <summary>
    /// Serialized HttpContent
    /// </summary>
    string? HttpContentInfo { get; }

    /// <summary>
    /// Error if response had error
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
    IError? Error { get; }

    /// <summary>
    /// Response if call was request
    /// </summary>
    IResponse? SingleResponse { get; }

    /// <summary>
    /// List of responses if call was batch
    /// </summary>
    List<IResponse>? BatchResponse { get; }

    /// <summary>
    /// Set request URL
    /// </summary>
    IJsonRpcCallContext WithRequestUrl(string? requestUrl);

    /// <summary>
    /// Set single call
    /// </summary>
    IJsonRpcCallContext WithSingle(IUntypedCall singleCall);

    /// <summary>
    /// Set batch call
    /// </summary>
    IJsonRpcCallContext WithBatch(ICollection<IUntypedCall> batchCall);

    /// <summary>
    /// Set HttpResponseInfo
    /// </summary>
    IJsonRpcCallContext WithHttpResponse(HttpResponseMessage httpResponseMessage);

    /// <summary>
    /// Set HttpContentInfo
    /// </summary>
    IJsonRpcCallContext WithHttpContent(HttpContent httpContent, string httpContentString);

    /// <summary>
    /// Set single response
    /// </summary>
    IJsonRpcCallContext WithSingleResponse(IResponse singleResponse);

    /// <summary>
    /// Set batch response
    /// </summary>
    IJsonRpcCallContext WithBatchResponse(List<IResponse> batchResponse);

    /// <summary>
    /// Set error
    /// </summary>
    /// <param name="untypedErrorResponse"></param>
    /// <returns></returns>
    IJsonRpcCallContext WithError(UntypedErrorResponse untypedErrorResponse);

    /// <summary></summary>
    [UsedImplicitly]
    string ToString();
}
