using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Features;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services;

internal class JsonRpcRequestHandler : IJsonRpcRequestHandler
{
    private readonly IJsonRpcExceptionWrapper exceptionWrapper;
    private readonly JsonRpcServerOptions options;

    public JsonRpcRequestHandler(IJsonRpcExceptionWrapper exceptionWrapper, IOptions<JsonRpcServerOptions> options)
    {
        this.exceptionWrapper = exceptionWrapper;
        this.options = options.Value;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to wrap all unexpected exceptions in json rpc response")]
    public async Task<IResponseWrapper?> ProcessJsonRpcRequest(IRequestWrapper? requestWrapper, HttpContext httpContext, RequestDelegate next)
    {
        try
        {
            return requestWrapper switch
            {
                SingleRequestWrapper singleRequestWrapper => await ProcessSingleRequest(httpContext, singleRequestWrapper, next),
                BatchRequestWrapper batchRequestWrapper => await ProcessBatchRequest(httpContext, batchRequestWrapper, next),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e)
        {
            var error = exceptionWrapper.WrapGeneralException(e);
            return new SingleResponseWrapper(error);
        }
    }

    private async Task<IResponseWrapper?> ProcessBatchRequest(HttpContext httpContext, BatchRequestWrapper batchRequestWrapper, RequestDelegate next)
    {
        if (batchRequestWrapper.Calls.Count == 0)
        {
            throw new JsonRpcFormatException("Empty batch call");
        }

        var responses = new List<IResponse>();
        foreach (var call in batchRequestWrapper.Calls)
        {
            var response = await ProcessCallSafe(httpContext, call, true, next);
            if (response != null)
            {
                responses.Add(response);
            }

            httpContext.SetEndpoint(null);
        }

        return responses.Count == 0
            ? null
            : new BatchResponseWrapper(responses);
    }

    private async Task<IResponseWrapper?> ProcessSingleRequest(HttpContext httpContext, SingleRequestWrapper singleRequestWrapper, RequestDelegate next)
    {
        var response = await ProcessCallSafe(httpContext, singleRequestWrapper.Call, false, next);
        return response == null
            ? null
            : new SingleResponseWrapper(response);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to wrap all unexpected exceptions in json rpc response")]
    private async Task<IResponse?> ProcessCallSafe(HttpContext callHttpContext, JsonDocument rawCall, bool isBatch, RequestDelegate next)
    {
        IUntypedCall? call = null;
        try
        {
            call = DeserializeCall(rawCall);
            callHttpContext.Features.Set(new JsonRpcFeature
            {
                RawCall = rawCall,
                Call = call,
                IsBatch = isBatch,
            });

            await next(callHttpContext);

            // retrieving it again in case it was replaced during request processing
            var feature = callHttpContext.Features.Get<IJsonRpcFeature>();
            if (feature == null)
            {
                throw new JsonRpcServerException($"{nameof(IJsonRpcFeature)} not found in HttpContext");
            }

            return feature.Response;
        }
        catch (Exception e)
        {
            if (call is UntypedNotification)
            {
                return null;
            }

            var request = call as UntypedRequest;
            return exceptionWrapper.WrapGeneralException(e, request?.Id);
        }
    }

    private IUntypedCall DeserializeCall(JsonDocument rawCall)
    {
        var call = rawCall.Deserialize<IUntypedCall>(options.HeadersJsonSerializerOptions)!;

        if (string.IsNullOrWhiteSpace(call.Method))
        {
            throw new JsonRpcFormatException("Method is null or empty");
        }

        if (call.Jsonrpc != JsonRpcConstants.Version)
        {
            throw new JsonRpcFormatException($"Only [{JsonRpcConstants.Version}] version supported. Got [{call.Jsonrpc}]");
        }

        return call;
    }
}
