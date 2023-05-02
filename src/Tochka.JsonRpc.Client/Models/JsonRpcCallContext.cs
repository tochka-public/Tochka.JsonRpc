using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

[PublicAPI]
public class JsonRpcCallContext : IJsonRpcCallContext
{
    public string? RequestUrl { get; private set; }
    public IUntypedCall? SingleCall { get; private set; }
    public List<IUntypedCall>? BatchCall { get; private set; }
    public int ExpectedBatchResponseCount { get; private set; }
    public string? HttpResponseInfo { get; private set; }
    public string? HttpContentInfo { get; private set; }
    public IResponse? SingleResponse { get; private set; }
    public List<IResponse>? BatchResponse { get; private set; }
    public IError? Error { get; private set; }

    public void WithRequestUrl(string? requestUrl)
    {
        if (requestUrl == null)
        {
            return;
        }

        if (requestUrl.StartsWith("/", StringComparison.Ordinal))
        {
            throw new ArgumentException("Request url should not start with '/' to prevent unexpected behavior when joining url parts", nameof(requestUrl));
        }

        RequestUrl = requestUrl;
    }

    public void WithSingle(IUntypedCall singleCall)
    {
        if (BatchCall != null)
        {
            throw new InvalidOperationException("Can't add single call when batch call is present");
        }

        SingleCall = singleCall;
    }

    public void WithBatch(List<IUntypedCall> batchCall)
    {
        if (SingleCall != null)
        {
            throw new InvalidOperationException("Can't add batch call when single call is present");
        }

        BatchCall = batchCall;
        ExpectedBatchResponseCount = batchCall.Count(static x => x is UntypedRequest);
    }

    public void WithHttpResponse(HttpResponseMessage httpResponseMessage)
    {
        HttpResponseInfo = $"{httpResponseMessage}";
        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            throw new JsonRpcException("Expected HTTP code 200", this);
        }
    }

    public void WithHttpContent(HttpContent httpContent, string httpContentString)
    {
        HttpContentInfo = GetStringWithLimit(httpContentString);
        if (httpContent == null)
        {
            throw new JsonRpcException("Response content is null", this);
        }

        var contentLength = httpContent.Headers.ContentLength;
        if (contentLength is null or 0)
        {
            throw new JsonRpcException($"Bad Content-Length [{contentLength}]", this);
        }
    }

    public void WithSingleResponse(IResponse singleResponse)
    {
        if (BatchResponse != null)
        {
            throw new InvalidOperationException("Can't add single response when batch response is present");
        }

        SingleResponse = singleResponse;
        if (singleResponse == null)
        {
            throw new JsonRpcException("Could not parse body as JSON Rpc response", this);
        }

        if (singleResponse.Jsonrpc != JsonRpcConstants.Version)
        {
            throw new JsonRpcException($"JSON Rpc response version is invalid: [{singleResponse.Jsonrpc}], expected [{JsonRpcConstants.Version}]", this);
        }

        if (BatchCall != null)
        {
            return;
        }

        if (SingleCall is not UntypedRequest request)
        {
            throw new JsonRpcException("Received response but call was not request or batch", this);
        }

        // from specification:
        // "If there was an error in detecting the id in the Request object (e.g. Parse error/Invalid Request), it MUST be Null."
        if (SingleResponse is UntypedErrorResponse { Id: NullRpcId })
        {
            return;
        }

        if (!singleResponse.Id.Equals(request.Id))
        {
            throw new JsonRpcException($"JSON Rpc response id is invalid: [{singleResponse.Id}], expected [{request.Id}] or [null]", this);
        }
    }

    public void WithBatchResponse(List<IResponse> batchResponse)
    {
        if (SingleResponse != null)
        {
            throw new InvalidOperationException("Can't add batch response when single response is present");
        }

        if (ExpectedBatchResponseCount == 0)
        {
            throw new InvalidOperationException("Can't add batch response when no response is expected");
        }

        BatchResponse = batchResponse;
        if (batchResponse == null)
        {
            throw new JsonRpcException("Could not parse body as JSON Rpc batch response", this);
        }

        if (batchResponse.Count == 0)
        {
            throw new JsonRpcException("Protocol violation: server should not return empty batches", this);
        }

        if (batchResponse.Count != ExpectedBatchResponseCount)
        {
            throw new JsonRpcException($"Batch JSON Rpc response has wrong count [{batchResponse.Count}], expected [{ExpectedBatchResponseCount}]", this);
        }

        if (batchResponse.Any(static x => x.Jsonrpc != JsonRpcConstants.Version))
        {
            throw new JsonRpcException($"Batch JSON Rpc response has item with invalid version, expected [{JsonRpcConstants.Version}]", this);
        }
    }

    public void WithError(UntypedErrorResponse untypedErrorResponse) => Error = untypedErrorResponse.Error;

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        var sb = new StringBuilder(nameof(JsonRpcCallContext));
        var emptyOutput = true;
        if (RequestUrl != null)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"    Request url [{RequestUrl}]");
            emptyOutput = false;
        }

        if (SingleCall != null)
        {
            sb.AppendLine();
            sb.AppendLine("    Single call:");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        {SingleCall}");
            emptyOutput = false;
        }

        if (BatchCall != null)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"    Batch call (count {BatchCall.Count}, expected response count {ExpectedBatchResponseCount}):");
            foreach (var x in BatchCall)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {x}");
            }

            emptyOutput = false;
        }

        if (HttpResponseInfo != null)
        {
            sb.AppendLine();
            sb.AppendLine("    HTTP response:");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        {HttpResponseInfo}");
            emptyOutput = false;
        }

        if (HttpContentInfo != null)
        {
            sb.AppendLine();
            sb.AppendLine("    HTTP response content:");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        {HttpContentInfo}");
            emptyOutput = false;
        }

        if (SingleResponse != null)
        {
            sb.AppendLine();
            sb.AppendLine("    Single JSON Rpc response:");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        {SingleResponse}");
            emptyOutput = false;
        }

        if (BatchResponse != null)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"    Batch JSON Rpc response (count={BatchResponse.Count}):");
            foreach (var x in BatchResponse)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"        {x}");
            }

            emptyOutput = false;
        }

        if (Error != null)
        {
            sb.AppendLine();
            sb.AppendLine("    JSON Rpc error:");
            sb.AppendLine(CultureInfo.InvariantCulture, $"        {Error}");
            emptyOutput = false;
        }

        if (emptyOutput)
        {
            sb.AppendLine("(nothing)");
        }

        return sb.ToString();
    }

    private static string GetStringWithLimit(string str) =>
        string.IsNullOrEmpty(str) || str.Length <= JsonRpcConstants.LogStringLimit
            ? str
            : str[..JsonRpcConstants.LogStringLimit];
}
