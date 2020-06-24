using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Models.Response;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services
{
    public class RequestHandler : IRequestHandler
    {
        private readonly IJsonRpcErrorFactory errorFactory;
        private readonly HeaderRpcSerializer headerRpcSerializer;
        private readonly INestedContextFactory nestedContextFactory;
        private readonly IResponseReader responseReader;
        private readonly ILogger log;
        private readonly JsonRpcOptions options;

        public RequestHandler(IJsonRpcErrorFactory errorFactory, HeaderRpcSerializer headerRpcSerializer, INestedContextFactory nestedContextFactory, IResponseReader responseReader, IOptions<JsonRpcOptions> options, ILogger<RequestHandler> log)
        {
            this.errorFactory = errorFactory;
            this.headerRpcSerializer = headerRpcSerializer;
            this.nestedContextFactory = nestedContextFactory;
            this.responseReader = responseReader;
            this.log = log;
            this.options = options.Value;
        }

        public virtual async Task HandleRequest(HttpContext httpContext, IRequestWrapper requestWrapper, Encoding requestEncoding, RequestDelegate next)
        {
            var context = new HandlingContext(httpContext, requestEncoding, next);
            try
            {
                log.LogTrace($"RequestWrapper is {requestWrapper?.GetType().Name}, encoding is {requestEncoding.EncodingName}");
                var responseWrapper = await HandleRequestWrapper(requestWrapper, context);
                await responseWrapper.Write(context, headerRpcSerializer);
            }
            catch (Exception e)
            {
                // paranoid safeguard against unexpected errors
                log.LogWarning(e, $"{nameof(HandleRequest)} failed, write error response");
                await HandleException(context, e);
            }
        }

        protected internal virtual async Task<Models.Response.IServerResponseWrapper> HandleRequestWrapper(IRequestWrapper requestWrapper, HandlingContext context)
        {
            try
            {
                switch (requestWrapper)
                {
                    case BadRequestWrapper badRequestWrapper:
                        return HandleBad(badRequestWrapper, context);
                    case BatchRequestWrapper batchRequestWrapper:
                        return await HandleBatch(batchRequestWrapper, options.BatchHandling, context);
                    case SingleRequestWrapper singleRequestWrapper:
                        return await HandleSingle(singleRequestWrapper, context);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(requestWrapper), requestWrapper?.GetType().Name);
                }
            }
            catch (Exception e)
            {
                // If the batch rpc call itself fails to be recognized as an valid JSON or as an Array with at least one value, the response from the Server MUST be a single Response object
                // i think even if we failed parsing and don't know if this is a notification, this is right
                context.WriteResponse = true;
                log.LogWarning(e, $"{nameof(HandleRequestWrapper)} failed, set writeResponse [{context.WriteResponse}], convert exception to error response");
                var response = errorFactory.ConvertExceptionToResponse(e, headerRpcSerializer);
                return GetResponseWrapper(response);
            }
        }

        protected internal virtual JsonServerResponseWrapper HandleBad(BadRequestWrapper badRequestWrapper, HandlingContext context)
        {
            log.LogTrace($"{nameof(HandleBad)}: converting exception to json response");
            context.WriteResponse = true;
            var error = GetError(badRequestWrapper.Exception);
            var errorResponse = new ErrorResponse<object>
            {
                Error = new Error<object>
                {
                    Code = error.Code,
                    Message = error.Message,
                    Data = error.GetData()
                }
            };
            var value = JToken.FromObject(errorResponse, headerRpcSerializer.Serializer);
            return new JsonServerResponseWrapper(value, null, null);
            // TODO any cases when need to pass through?
            // TODO it can break protocol because we can not distinguish between bad rpc request and absense of rpc route/action, for example
        }

        protected internal virtual IError GetError(Exception exception)
        {
            switch (exception)
            {
                case JsonRpcInternalException _:
                case ArgumentOutOfRangeException _:
                    return errorFactory.InvalidRequest(exception);
                case Newtonsoft.Json.JsonException _:
                    return errorFactory.ParseError(exception);
                default:
                    return errorFactory.Exception(exception);
            }
        }

        protected internal virtual async Task<IServerResponseWrapper> HandleSingle(SingleRequestWrapper singleRequestWrapper, HandlingContext context)
        {
            context.WriteResponse = singleRequestWrapper.Call is UntypedRequest;
            log.LogTrace($"{nameof(HandleSingle)}: set writeResponse [{context.WriteResponse}] because call is request");
            return await SafeNext(singleRequestWrapper.Call, context, options.AllowRawResponses);
        }

        protected internal virtual async Task<IServerResponseWrapper> HandleBatch(BatchRequestWrapper batchRequestWrapper, BatchHandling batchHandling, HandlingContext context)
        {
            // If the batch rpc call itself fails to be recognized as an valid JSON or as an Array with at least one value, the response from the Server MUST be a single Response object
            if (batchRequestWrapper.Batch.Count == 0)
            {
                throw new JsonRpcInternalException("JSON Rpc batch request is empty");
            }
            // the server MUST NOT return an empty Array and should return nothing at all
            var hasRequestInBatch = batchRequestWrapper.Batch.OfType<UntypedRequest>().Any();
            context.WriteResponse = hasRequestInBatch;
            log.LogTrace($"{nameof(HandleBatch)}: set writeResponse [{context.WriteResponse}] because {nameof(hasRequestInBatch)} is [{hasRequestInBatch}]");
            // We ignore headers and other properties from nested responses because there is no good way to solve possible conflicts
            switch (batchHandling)
            {
                case BatchHandling.Sequential:
                    var batchResponse = await HandleBatchSequential(batchRequestWrapper, context);
                    return new JsonServerResponseWrapper(batchResponse, null, null);

                default:
                    throw new ArgumentOutOfRangeException(nameof(options.BatchHandling), options.BatchHandling, "Unsupported value");
            }
        }

        protected internal virtual async Task HandleException(HandlingContext context, Exception e)
        {
            context.WriteResponse = true;
            log.LogTrace($"{nameof(HandleException)}: set writeResponse [{context.WriteResponse}]");
            var response = errorFactory.ConvertExceptionToResponse(e, headerRpcSerializer);
            var wrapper = GetResponseWrapper(response);
            await wrapper.Write(context, headerRpcSerializer);
        }

        protected internal virtual IServerResponseWrapper GetResponseWrapper(JToken response)
        {
            return new JsonServerResponseWrapper(response, null, null);
        }

        protected internal virtual async Task<JArray> HandleBatchSequential(BatchRequestWrapper batchRequestWrapper, HandlingContext context)
        {
            var batchResponse = new JArray();
            var i = 1;
            foreach (var call in batchRequestWrapper.Batch)
            {
                log.LogTrace($"{nameof(HandleBatchSequential)}: [{i}/{batchRequestWrapper.Batch.Count}] processing");
                var response = await GetResponseSafeInBatch(call, context);
                if (call is UntypedRequest)
                {
                    batchResponse.Add(response);
                    log.LogTrace($"{nameof(HandleBatchSequential)}: [{i}/{batchRequestWrapper.Batch.Count}] add response to result because call was request");
                }
                else
                {
                    log.LogTrace($"{nameof(HandleBatchSequential)}: [{i}/{batchRequestWrapper.Batch.Count}] ignored response because call was not request");
                }
            }

            return batchResponse;
        }

        protected internal virtual async Task<JToken> GetResponseSafeInBatch(IUntypedCall call, HandlingContext context)
        {
            try
            {
                var response = await SafeNext(call, context, false);
                switch (response)
                {
                    case JsonServerResponseWrapper jsonResponseWrapper:
                        return jsonResponseWrapper.Value;
                    case RawServerResponseWrapper rawResponseWrapper:
                        throw new JsonRpcInternalException("Raw responses are not supported in batches", null, null, rawResponseWrapper.Source.StatusCode);
                    case null:
                        throw new ArgumentNullException(nameof(response));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name);
                }
            }
            catch (Exception e)
            {
                log.LogWarning(e, $"{nameof(GetResponseSafeInBatch)} failed: converting exception to json response");
                return errorFactory.ConvertExceptionToResponse(e, headerRpcSerializer);
            }
        }

        protected internal virtual async Task<IServerResponseWrapper> SafeNext(IUntypedCall call, HandlingContext context, bool allowRawResponses)
        {
            log.LogTrace($"{nameof(SafeNext)}: Started. {nameof(allowRawResponses)} is [{allowRawResponses}]");
            IHeaderDictionary nestedHeaders = null;
            try
            {
                context.OriginalHttpContext.RequestAborted.ThrowIfCancellationRequested();
                var nestedHttpContext = nestedContextFactory.Create(context.OriginalHttpContext, call, context.RequestEncoding);
                log.LogTrace($"{nameof(SafeNext)}: invoking pipeline on nested context");
                await context.Next(nestedHttpContext);
                nestedHeaders = nestedHttpContext.Response.Headers;
                var result = await responseReader.GetResponse(nestedHttpContext, call, allowRawResponses, context.OriginalHttpContext.RequestAborted);
                if (result == null)
                {
                    throw new JsonRpcInternalException($"{nameof(ResponseReader)} returned null");
                }
                log.LogTrace($"{nameof(SafeNext)}: Completed");
                return result;

            }
            catch (Exception e)
            {
                log.LogWarning(e, $"{nameof(SafeNext)} failed: converting exception to json response");
                var response = errorFactory.ConvertExceptionToResponse(e, headerRpcSerializer);
                return new JsonServerResponseWrapper(response, call, nestedHeaders);
            }
        }
    }
}