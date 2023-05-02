using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Features;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server;

public class JsonRpcMiddleware
{
    private readonly RequestDelegate next;
    private readonly IJsonRpcErrorFactory errorFactory;
    private readonly JsonRpcServerOptions options;

    public JsonRpcMiddleware(RequestDelegate next, IJsonRpcErrorFactory errorFactory, IOptions<JsonRpcServerOptions> options)
    {
        this.next = next;
        this.errorFactory = errorFactory;
        this.options = options.Value;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!httpContext.IsJsonRpcRequest(options.RoutePrefix))
        {
            await next(httpContext);
            return;
        }

        var responseWrapper = await ProcessJsonRpcRequest(httpContext);
        if (responseWrapper != null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            // TODO is it ok to always return utf-8?
            httpContext.Response.ContentType = $"{JsonRpcConstants.ContentType}; charset=utf-8";
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, responseWrapper, options.HeadersJsonSerializerOptions);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to wrap all unexpected parsing exceptions in json rpc response")]
    private async Task<IResponseWrapper?> ProcessJsonRpcRequest(HttpContext httpContext)
    {
        try
        {
            // TODO what if it is not utf-8?
            var body = httpContext.Request.Body;
            var requestWrapper = await JsonSerializer.DeserializeAsync<IRequestWrapper>(body, options.HeadersJsonSerializerOptions);
            return requestWrapper switch
            {
                SingleRequestWrapper singleRequestWrapper => await ProcessSingleRequest(httpContext, singleRequestWrapper),
                BatchRequestWrapper batchRequestWrapper => await ProcessBatchRequest(httpContext, batchRequestWrapper),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e)
        {
            var error = e is JsonException
                ? WrapParseException(e)
                : WrapException(e);
            return new SingleResponseWrapper(error);
        }
    }

    private async Task<IResponseWrapper?> ProcessBatchRequest(HttpContext httpContext, BatchRequestWrapper batchRequestWrapper)
    {
        if (batchRequestWrapper.Calls.Count == 0)
        {
            throw new JsonRpcFormatException("Empty batch call");
        }

        var responses = new List<IResponse>();
        foreach (var call in batchRequestWrapper.Calls)
        {
            var response = await ProcessCallSafe(httpContext, call, true);
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

    private async Task<IResponseWrapper?> ProcessSingleRequest(HttpContext httpContext, SingleRequestWrapper singleRequestWrapper)
    {
        var response = await ProcessCallSafe(httpContext, singleRequestWrapper.Call, false);
        return response == null
            ? null
            : new SingleResponseWrapper(response);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to wrap all unexpected exceptions in json rpc response")]
    private async Task<IResponse?> ProcessCallSafe(HttpContext callHttpContext, JsonDocument rawCall, bool isBatch)
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
            return WrapException(e, request?.Id);
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

    private UntypedErrorResponse WrapException(Exception exception, IRpcId? id = null)
    {
        var error = errorFactory.Exception(exception);
        return new UntypedErrorResponse(id ?? new NullRpcId(), error.AsUntypedError(options.HeadersJsonSerializerOptions));
    }

    private UntypedErrorResponse WrapParseException(Exception exception)
    {
        var error = errorFactory.ParseError(exception);
        return new UntypedErrorResponse(new NullRpcId(), error.AsUntypedError(options.HeadersJsonSerializerOptions));
    }
}
