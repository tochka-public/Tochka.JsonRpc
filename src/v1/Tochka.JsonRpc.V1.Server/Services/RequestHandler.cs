using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.V1.Common.Models.Response;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server.Exceptions;
using Tochka.JsonRpc.V1.Server.Models.Response;
using Tochka.JsonRpc.V1.Server.Settings;

namespace Tochka.JsonRpc.V1.Server.Services
{
    public class RequestHandler : IRequestHandler
    {
        private readonly IJsonRpcErrorFactory errorFactory;
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;
        private readonly INestedContextFactory nestedContextFactory;
        private readonly IResponseReader responseReader;
        private readonly ILogger log;
        private readonly JsonRpcOptions options;

        public RequestHandler(IJsonRpcErrorFactory errorFactory, HeaderJsonRpcSerializer headerJsonRpcSerializer, INestedContextFactory nestedContextFactory, IResponseReader responseReader, IOptions<JsonRpcOptions> options, ILogger<RequestHandler> log)
        {
            this.errorFactory = errorFactory;
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
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
                log.LogTrace("RequestWrapper is {requestWrapperTypeName}, encoding is {requestEncodingName}", requestWrapper?.GetType().Name, requestEncoding.EncodingName);
                var responseWrapper = await HandleRequestWrapper(requestWrapper, context);
                await responseWrapper.Write(context, headerJsonRpcSerializer);
            }
            catch (Exception e)
            {
                // paranoid safeguard against unexpected errors
                log.LogWarning(e, "{action} failed, write error response", nameof(HandleRequest));
                await HandleException(context, e);
            }
        }

        protected internal virtual async Task<IServerResponseWrapper> HandleRequestWrapper(IRequestWrapper requestWrapper, HandlingContext context)
        {
            try
            {
                return requestWrapper switch
                {
                    BadRequestWrapper badRequestWrapper => HandleBad(badRequestWrapper, context),
                    BatchRequestWrapper batchRequestWrapper => await HandleBatch(batchRequestWrapper, options.BatchHandling, context),
                    SingleRequestWrapper singleRequestWrapper => await HandleSingle(singleRequestWrapper, context),
                    _ => throw new ArgumentOutOfRangeException(nameof(requestWrapper), requestWrapper?.GetType().Name)
                };
            }
            catch (Exception e)
            {
                // If the batch rpc call itself fails to be recognized as an valid JSON or as an Array with at least one value, the response from the Server MUST be a single Response object
                // i think even if we failed parsing and don't know if this is a notification, this is right
                context.WriteResponse = true;

                log.LogWarning(e,
                               "{action} failed, set writeResponse [{contextWriteResponse}], convert exception to error response",
                               nameof(HandleRequestWrapper),
                               context.WriteResponse);
                
                var response = errorFactory.ConvertExceptionToResponse(e, headerJsonRpcSerializer);
                return GetResponseWrapper(response);
            }
        }

        protected internal virtual JsonServerResponseWrapper HandleBad(BadRequestWrapper badRequestWrapper, HandlingContext context)
        {
            log.LogTrace("{action}: converting exception to json response", nameof(HandleBad));
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
            var value = JToken.FromObject(errorResponse, headerJsonRpcSerializer.Serializer);
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
            log.LogTrace("{action}: set writeResponse [{contextWriteResponse}] because call is request", nameof(HandleSingle), context.WriteResponse);
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

            log.LogTrace("{action}: set writeResponse [{contextWriteResponse}] because {hasRequestInBatchName} is [{hasRequestInBatch}]",
                         nameof(HandleBatch),
                         context.WriteResponse,
                         nameof(hasRequestInBatch),
                         hasRequestInBatch);
            
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
            log.LogTrace("{action}: set writeResponse [{contextWriteResponse}]", nameof(HandleException), context.WriteResponse);
            var response = errorFactory.ConvertExceptionToResponse(e, headerJsonRpcSerializer);
            var wrapper = GetResponseWrapper(response);
            await wrapper.Write(context, headerJsonRpcSerializer);
        }

        protected internal virtual IServerResponseWrapper GetResponseWrapper(JToken response)
        {
            return new JsonServerResponseWrapper(response, null, null);
        }

        protected internal virtual async Task<JArray> HandleBatchSequential(BatchRequestWrapper batchRequestWrapper, HandlingContext context)
        {
            var batchResponse = new JArray();
            var index = 1;
            
            foreach (var call in batchRequestWrapper.Batch)
            {
                log.LogTrace("{action}: [{index}/{batchCount}] processing", nameof(HandleBatchSequential), index, batchRequestWrapper.Batch.Count);
                
                var response = await GetResponseSafeInBatch(call, context);
                
                if (call is UntypedRequest)
                {
                    batchResponse.Add(response);

                    log.LogTrace("{action}: [{index}/{batchCount}] add response to result because call was request",
                                 nameof(HandleBatchSequential),
                                 index,
                                 batchRequestWrapper.Batch.Count);
                }
                else
                {
                    log.LogTrace("{action}: [{index}/{batchCount}] ignored response because call was not request",
                                 nameof(HandleBatchSequential),
                                 index,
                                 batchRequestWrapper.Batch.Count);
                }

                index++;
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
                if (e is not JsonRpcErrorResponseException)
                {
                    log.LogWarning(e, "{action} failed: converting exception to json response", nameof(GetResponseSafeInBatch));
                }
                return errorFactory.ConvertExceptionToResponse(e, headerJsonRpcSerializer);
            }
        }

        protected internal virtual async Task<IServerResponseWrapper> SafeNext(IUntypedCall call, HandlingContext context, bool allowRawResponses)
        {
            log.LogTrace("{action}: Started. {allowRawResponsesName} is [{allowRawResponses}]",
                         nameof(SafeNext),
                         nameof(allowRawResponses),
                         allowRawResponses);
            
            IHeaderDictionary nestedHeaders = null;
            HttpContext nestedHttpContext = null;
            try
            {
                context.OriginalHttpContext.RequestAborted.ThrowIfCancellationRequested();
                nestedHttpContext = nestedContextFactory.Create(context.OriginalHttpContext, call, context.RequestEncoding);
                
                log.LogTrace("{action}: invoking pipeline on nested context", nameof(SafeNext));
                
                await context.Next(nestedHttpContext);
                PropagateItems(context.OriginalHttpContext, nestedHttpContext);
                nestedHeaders = nestedHttpContext.Response.Headers;
                var result = await responseReader.GetResponse(nestedHttpContext, call, allowRawResponses, context.OriginalHttpContext.RequestAborted);
                if (result == null)
                {
                    throw new JsonRpcInternalException($"{nameof(ResponseReader)} returned null");
                }

                log.LogTrace("{action}: Completed", nameof(SafeNext));
                
                return result;

            }
            catch (Exception e)
            {
                if (e is not JsonRpcErrorResponseException)
                {
                    log.LogWarning(e, "{action} failed: converting exception to json response", nameof(SafeNext));
                }

                PropagateItems(context.OriginalHttpContext, nestedHttpContext);
                var response = errorFactory.ConvertExceptionToResponse(e, headerJsonRpcSerializer);
                return new JsonServerResponseWrapper(response, call, nestedHeaders);
            }
        }

        internal void PropagateItems(HttpContext context, HttpContext nestedHttpContext)
        {
            if (PropagateItemsInternal(context, nestedHttpContext, JsonRpcConstants.ActionDescriptorItemKey))
            {
                log.LogTrace("Propagated item to original HttpContext: {actionTypeKey}", nameof(JsonRpcConstants.ActionDescriptorItemKey));
            }

            if (PropagateItemsInternal(context, nestedHttpContext, JsonRpcConstants.ActionResultTypeItemKey))
            {
                log.LogTrace("Propagated item to original HttpContext: {actionTypeKey}", nameof(JsonRpcConstants.ActionResultTypeItemKey));
            }

            if (PropagateItemsInternal(context, nestedHttpContext, JsonRpcConstants.ResponseErrorCodeItemKey))
            {
                log.LogTrace("Propagated item to original HttpContext: {actionTypeKey}", nameof(JsonRpcConstants.ResponseErrorCodeItemKey));
            }
        }

        /// <summary>
        /// Copies JsonRpc related items to original HttpContext. Sets null values on conflict (in batches).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="nestedHttpContext"></param>
        /// <param name="itemKey"></param>
        /// <returns></returns>
        internal virtual bool PropagateItemsInternal(HttpContext context, HttpContext nestedHttpContext, object itemKey)
        {
            if (nestedHttpContext == null)
            {
                return false;
            }

            if (!nestedHttpContext.Items.ContainsKey(itemKey))
            {
                return false;
            }

            if (context.Items.ContainsKey(itemKey))
            {
                // This is batch and somebody has already set value. Override with null to avoid conflicts. 
                context.Items[itemKey] = null;
                return false;
            }

            context.Items[itemKey] = nestedHttpContext.Items[itemKey];
            return true;
        }
    }
}
