using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server;

public class JsonRpcMiddleware
{
    private readonly RequestDelegate next;
    private readonly IJsonRpcRequestHandler requestHandler;
    private readonly IJsonRpcExceptionWrapper exceptionWrapper;
    private readonly JsonRpcServerOptions options;

    public JsonRpcMiddleware(RequestDelegate next, IJsonRpcRequestHandler requestHandler, IJsonRpcExceptionWrapper exceptionWrapper, IOptions<JsonRpcServerOptions> options)
    {
        this.next = next;
        this.requestHandler = requestHandler;
        this.exceptionWrapper = exceptionWrapper;
        this.options = options.Value;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!httpContext.IsJsonRpcRequest(options.RoutePrefix))
        {
            await next(httpContext);
            return;
        }

        var requestEncoding = httpContext.Request.GetTypedHeaders().ContentType?.Encoding ?? Encoding.UTF8;
        var responseWrapper = await ProcessJsonRpcRequest(httpContext, requestEncoding);
        if (responseWrapper != null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            var contentType = new MediaTypeHeaderValue(JsonRpcConstants.ContentType) { Encoding = requestEncoding };
            httpContext.Response.ContentType = contentType.ToString();
            await SerializeResponseWrapper(responseWrapper, httpContext.Response.Body, requestEncoding);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to wrap all unexpected parsing exceptions in json rpc response")]
    private async Task<IResponseWrapper?> ProcessJsonRpcRequest(HttpContext httpContext, Encoding requestEncoding)
    {
        try
        {
            var requestWrapper = await DeserializeRequestWrapper(httpContext.Request.Body, requestEncoding);
            return await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, next);
        }
        catch (Exception e)
        {
            var error = e is JsonException
                ? exceptionWrapper.WrapParseException(e)
                : exceptionWrapper.WrapGeneralException(e);
            return new SingleResponseWrapper(error);
        }
    }

    private async Task<IRequestWrapper?> DeserializeRequestWrapper(Stream requestBody, Encoding requestEncoding)
    {
        if (requestEncoding.CodePage == Encoding.UTF8.CodePage)
        {
            return await JsonSerializer.DeserializeAsync<IRequestWrapper>(requestBody, options.HeadersJsonSerializerOptions);
        }

        await using var transcodingStream = Encoding.CreateTranscodingStream(requestBody, requestEncoding, Encoding.UTF8, true);
        return await JsonSerializer.DeserializeAsync<IRequestWrapper>(transcodingStream, options.HeadersJsonSerializerOptions);
    }

    private async Task SerializeResponseWrapper(IResponseWrapper responseWrapper, Stream responseBody, Encoding responseEncoding)
    {
        if (responseEncoding.CodePage == Encoding.UTF8.CodePage)
        {
            await JsonSerializer.SerializeAsync(responseBody, responseWrapper, options.HeadersJsonSerializerOptions);
            return;
        }

        await using var transcodingStream = Encoding.CreateTranscodingStream(responseBody, responseEncoding, Encoding.UTF8, true);
        await JsonSerializer.SerializeAsync(transcodingStream, responseWrapper, options.HeadersJsonSerializerOptions);
    }
}
