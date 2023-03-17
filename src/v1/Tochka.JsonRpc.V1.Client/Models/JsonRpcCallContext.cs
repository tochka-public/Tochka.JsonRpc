using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Models.Response;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;
using Tochka.JsonRpc.V1.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.V1.Client.Models
{
    public class JsonRpcCallContext : IJsonRpcCallContext
    {
        public string RequestUrl { get; private set; }

        public IUntypedCall SingleCall { get; private set; }

        public List<IUntypedCall> BatchCall { get; private set; }

        public int ExpectedBatchResponseCount { get; private set; }

        public string HttpResponseInfo { get; private set; }

        public string HttpContentInfo { get; private set; }

        public IResponse SingleResponse { get; private set; }

        public List<IResponse> BatchResponse { get; private set; }
        public IError Error { get; private set; }
        public string ErrorInfo { get; private set; }

        public void WithRequestUrl(string requestUrl)
        {
            if (requestUrl == null)
            {
                return;
            }

            if (requestUrl.StartsWith("/"))
            {
                throw new ArgumentException("Request url should not start with '/' to prevent unexpected behavior when joining url parts");
            }

            RequestUrl = requestUrl;
        }

        public void WithSingle(IUntypedCall singleCall)
        {
            SingleCall = singleCall;
        }

        public void WithBatch(List<IUntypedCall> batchCall)
        {
            BatchCall = batchCall;
            ExpectedBatchResponseCount = batchCall.Count(x => x is UntypedRequest);
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
            if (contentLength == null || contentLength == 0)
            {
                throw new JsonRpcException($"Bad Content-Length [{contentLength}]", this);
            }
        }

        public void WithSingleResponse(IResponse singleResponse)
        {
            if (BatchResponse != null)
            {
                throw new InvalidOperationException("Can not add single response when batch response is present");
            }

            SingleResponse = singleResponse;
            if (singleResponse == null)
            {
                throw new JsonRpcException($"Could not parse body as JSON Rpc response", this);
            }

            if (singleResponse.Jsonrpc != JsonRpcConstants.Version)
            {
                throw new JsonRpcException($"JSON Rpc response version is invalid: [{singleResponse.Jsonrpc}], expected [{JsonRpcConstants.Version}]", this);
            }

            if (BatchCall != null)
            {
                return;
            }

            if (!(SingleCall is UntypedRequest request))
            {
                throw new JsonRpcException($"Received response but call was not request or batch", this);
            }

            var idsAreNull = singleResponse.Id == null && request.Id == null;
            var idsAreEqual = singleResponse.Id?.Equals(request.Id) ?? false;
            if (!idsAreNull && !idsAreEqual)
            {
                throw new JsonRpcException($"JSON Rpc response id is invalid: [{singleResponse.Id}], expected [{request.Id}]", this);
            }
        }

        public void WithBatchResponse(List<IResponse> batchResponse)
        {
            if (SingleResponse != null)
            {
                throw new InvalidOperationException("Can not add batch response when single response is present");
            }

            if (ExpectedBatchResponseCount == 0)
            {
                throw new InvalidOperationException("Can not add batch response when no response is expected");
            }

            BatchResponse = batchResponse;
            if (batchResponse == null)
            {
                throw new JsonRpcException($"Could not parse body as JSON Rpc batch response", this);
            }

            if (batchResponse.Count == 0)
            {
                throw new JsonRpcException($"Protocol violation: server should not return empty batches", this);
            }

            if (batchResponse.Count != ExpectedBatchResponseCount)
            {
                throw new JsonRpcException($"Batch JSON Rpc response has wrong count [{batchResponse.Count}], expected [{ExpectedBatchResponseCount}]", this);
            }

            if (batchResponse.Any(x => x.Jsonrpc != JsonRpcConstants.Version))
            {
                throw new JsonRpcException($"Batch JSON Rpc response has item with invalid version, expected [{JsonRpcConstants.Version}]", this);
            }
        }

        public void WithError(UntypedErrorResponse untypedErrorResponse)
        {
            ErrorInfo = GetStringWithLimit(untypedErrorResponse.RawError);
            Error = untypedErrorResponse.Error;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sb = new StringBuilder(nameof(JsonRpcCallContext));
            var emptyOutput = true;
            if (RequestUrl != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    Request url [{RequestUrl}]");
                emptyOutput = false;
            }

            if (SingleCall != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    Single call:");
                sb.AppendLine($"        {SingleCall}");
                emptyOutput = false;
            }

            if (BatchCall != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    Batch call (count {BatchCall.Count}, expected response count {ExpectedBatchResponseCount}):");
                foreach (var x in BatchCall)
                {
                    sb.AppendLine($"        {x}");
                }

                emptyOutput = false;
            }

            if (HttpResponseInfo != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    HTTP response:");
                sb.AppendLine($"        {HttpResponseInfo}");
                emptyOutput = false;
            }

            if (HttpContentInfo != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    HTTP response content:");
                sb.AppendLine($"        {HttpContentInfo}");
                emptyOutput = false;
            }

            if (SingleResponse != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    Single JSON Rpc response:");
                sb.AppendLine($"        {SingleResponse}");
                emptyOutput = false;
            }

            if (BatchResponse != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    Batch JSON Rpc response (count={BatchResponse.Count}):");
                foreach (var x in BatchResponse)
                {
                    sb.AppendLine($"        {x}");
                }

                emptyOutput = false;
            }

            if (ErrorInfo != null)
            {
                sb.AppendLine();
                sb.AppendLine($"    JSON Rpc error:");
                sb.AppendLine($"        {ErrorInfo}");
                emptyOutput = false;
            }

            if (emptyOutput)
            {
                sb.AppendLine("(nothing)");
            }

            return sb.ToString();
        }

        private string GetStringWithLimit(string str) =>
            string.IsNullOrEmpty(str) || str.Length <= JsonRpcConstants.LogStringLimit
                ? str
                : str.Substring(0, JsonRpcConstants.LogStringLimit - 1);
    }
}
