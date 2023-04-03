using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Server.Pipeline;

public class JsonRpcMiddleware
{
    private readonly RequestDelegate next;
    private readonly IRequestReader requestReader;
    private readonly ILogger log;

    public JsonRpcMiddleware(RequestDelegate next, IRequestReader requestReader, ILogger<JsonRpcMiddleware> log)
    {
        this.next = next;
        this.requestReader = requestReader;
        this.log = log;
    }

    public async Task Invoke(HttpContext context, IRequestHandler requestHandler, IJsonRpcRoutes jsonRpcRoutes)
    {
        log.LogTrace("{action} of {middleware} Started", nameof(Invoke), nameof(JsonRpcMiddleware));
        var contentType = context.Request.GetTypedHeaders().ContentType;
        if (!Utils.ProbablyIsJsonRpc(context.Request, contentType))
        {
            log.LogTrace("Request is not recognized as JSON Rpc");
            await next(context);
            return;
        }

        if (!jsonRpcRoutes.IsJsonRpcRoute(context.Request.Path))
        {
            log.LogTrace("Request path is not registered as JSON Rpc");
            await next(context);
            return;
        }

        // TODO properly handle RESPONSE encoding as TextOutputFormatter does
        var requestEncoding = contentType?.Encoding ?? Encoding.UTF8;
        var requestWrapper = await requestReader.GetRequestWrapper(context, requestEncoding);
        await requestHandler.HandleRequest(context, requestWrapper, requestEncoding, next);
        log.LogTrace("{action} of {middleware} Completed", nameof(Invoke), nameof(JsonRpcMiddleware));
    }
}