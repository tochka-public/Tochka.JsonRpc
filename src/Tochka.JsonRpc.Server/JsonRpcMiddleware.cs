using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Server;

public class JsonRpcMiddleware
{
    private readonly RequestDelegate next;

    public JsonRpcMiddleware(RequestDelegate next) => this.next = next;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!IsJsonRpcRequest(httpContext, JsonRpcConstants.DefaultRoutePrefix))
        {
            await next(httpContext);
            return;
        }

        var body = httpContext.Request.Body;
        var wrapper = await JsonSerializer.DeserializeAsync<IRequestWrapper>(body, JsonRpcSerializerOptions.Headers);
        switch (wrapper)
        {
            case SingleRequestWrapper singleRequestWrapper:
                httpContext.AddJsonRpcCall(singleRequestWrapper.Call);
                await next(httpContext);
                break;
        }
    }

    private static bool IsJsonRpcRequest(HttpContext httpContext, PathString jsonRpcPrefix)
    {
        if (!httpContext.Request.Path.StartsWithSegments(jsonRpcPrefix))
        {
            return false;
        }

        if (httpContext.Request.Method != HttpMethods.Post)
        {
            return false;
        }

        var contentType = httpContext.Request.GetTypedHeaders().ContentType;
        if (contentType == null)
        {
            return false;
        }

        if (!contentType.MediaType.Equals(JsonRpcConstants.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
