using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Server;

public class JsonRpcMiddleware
{
    private readonly RequestDelegate next;
    private readonly JsonRpcServerOptions options;

    public JsonRpcMiddleware(RequestDelegate next, IOptions<JsonRpcServerOptions> options)
    {
        this.next = next;
        this.options = options.Value;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!httpContext.IsJsonRpcRequest(options.RoutePrefix))
        {
            await next(httpContext);
            return;
        }

        // todo process encoding (Encoding.CreateTranscodingStream)
        var body = httpContext.Request.Body;
        var wrapper = await JsonSerializer.DeserializeAsync<IRequestWrapper>(body, JsonRpcSerializerOptions.Headers);
        switch (wrapper)
        {
            case SingleRequestWrapper singleRequestWrapper:
                httpContext.SetJsonRpcCall(singleRequestWrapper.Call);
                await next(httpContext);
                break;
        }
    }
}
