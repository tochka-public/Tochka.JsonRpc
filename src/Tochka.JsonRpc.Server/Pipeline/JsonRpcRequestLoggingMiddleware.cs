using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Server.Pipeline;

public class JsonRpcRequestLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<JsonRpcRequestLoggingMiddleware> log;

    public JsonRpcRequestLoggingMiddleware(RequestDelegate next, ILogger<JsonRpcRequestLoggingMiddleware> log)
    {
        this.next = next;
        this.log = log;
    }

    public async Task Invoke(HttpContext context)
    {
        log.LogTrace("{action} of {middleware} Started", nameof(Invoke), nameof(JsonRpcRequestLoggingMiddleware));
        var call = context.GetJsonRpcCall();
        switch (call)
        {
            case UntypedNotification untypedNotification:
                log.LogInformation("JsonRpc notification [{method}]. Raw json: [{rawJson}]", untypedNotification.Method, untypedNotification.RawJson);
                break;
            case UntypedRequest untypedRequest:
                log.LogInformation("JsonRpc request [{method}], id [{id}]. Raw json: [{rawJson}]",
                    untypedRequest.Method,
                    untypedRequest.Id,
                    untypedRequest.RawJson);
                break;
        }

        await next(context);
        log.LogTrace("{action} of {middleware} Completed", nameof(Invoke), nameof(JsonRpcRequestLoggingMiddleware));
    }
}