using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
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

        // what if it is not utf-8?
        var body = httpContext.Request.Body;
        var requestWrapper = await JsonSerializer.DeserializeAsync<IRequestWrapper>(body, options.HeadersJsonSerializerOptions);
        var responseWrapper = await ProcessJsonRpcRequest(httpContext, requestWrapper);
        if (responseWrapper != null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            // is it ok to always return utf-8?
            httpContext.Response.ContentType = $"{JsonRpcConstants.ContentType}; charset=utf-8";
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, responseWrapper, options.HeadersJsonSerializerOptions);
        }
    }

    private async Task<IResponseWrapper?> ProcessJsonRpcRequest(HttpContext httpContext, IRequestWrapper? requestWrapper) =>
        requestWrapper switch
        {
            SingleRequestWrapper singleRequestWrapper => await ProcessSingleRequest(httpContext, singleRequestWrapper),
            BatchRequestWrapper batchRequestWrapper => await ProcessBatchRequest(httpContext, batchRequestWrapper),
            _ => throw new ArgumentOutOfRangeException()
        };

    private async Task<IResponseWrapper?> ProcessBatchRequest(HttpContext httpContext, BatchRequestWrapper batchRequestWrapper)
    {
        var responses = new List<IResponse>();
        foreach (var call in batchRequestWrapper.Calls)
        {
            var response = await ProcessCall(httpContext, call, true);
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
        var response = await ProcessCall(httpContext, singleRequestWrapper.Call, false);
        return response == null
            ? null
            : new SingleResponseWrapper(response);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to wrap all unexpected exceptions in json rpc response")]
    private async Task<IResponse?> ProcessCall(HttpContext callHttpContext, ICall call, bool isBatch)
    {
        callHttpContext.Features.Set(new JsonRpcFeature
        {
            Call = call,
            IsBatch = isBatch
        });
        try
        {
            await next(callHttpContext);
            var feature = callHttpContext.Features.Get<JsonRpcFeature>();
            if (feature == null)
            {
                throw new JsonRpcServerException($"{nameof(JsonRpcFeature)} not found in HttpContext, it's forbidden to clear it during processing of json rpc call");
            }

            return feature.Response;
        }
        catch (Exception e)
        {
            return WrapException(e, call);
        }
    }

    private IResponse? WrapException(Exception exception, ICall call)
    {
        if (call is not UntypedRequest request)
        {
            return null;
        }

        var error = errorFactory.Exception(exception);
        var untypedError = new Error<JsonDocument>(error.Code, error.Message, JsonSerializer.SerializeToDocument(error.Data, options.HeadersJsonSerializerOptions));
        return new UntypedErrorResponse(request.Id, untypedError);
    }
}
