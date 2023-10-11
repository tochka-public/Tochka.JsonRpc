using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Extensions;

namespace Tochka.JsonRpc.Server.Middlewares;

[ExcludeFromCodeCoverage(Justification = "Don't want to test logs")]
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
        var rawCall = context.GetRawJsonRpcCall();
        if (rawCall is not null)
        {
            log.LogInformation("JsonRpc request [{$rawJson}]", rawCall.RootElement);
        }

        await next(context);
        log.LogTrace("{action} of {middleware} Completed", nameof(Invoke), nameof(JsonRpcRequestLoggingMiddleware));
    }
}
