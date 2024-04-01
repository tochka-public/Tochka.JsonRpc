using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Server.Extensions;

namespace Tochka.JsonRpc.Server.Middlewares;

/// <summary>
/// Middleware for logging JsonRpc errors from responses like <see cref="UntypedErrorResponse" />
/// <remarks>Connection is optional</remarks>
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Don't want to test logs")]
public class JsonRpcErrorLoggingMiddleware
{
    private readonly ILogger<JsonRpcErrorLoggingMiddleware> logger;
    private readonly RequestDelegate next;

    /// <summary> </summary>
    public JsonRpcErrorLoggingMiddleware(RequestDelegate next, ILogger<JsonRpcErrorLoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary></summary>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        await next(httpContext);

        var response = httpContext.GetJsonRpcResponse() as UntypedErrorResponse;
        if (response?.Error != null)
        {
            logger.LogInformation("JsonRpc Error: Id = {ResponseId}, Code = {Code}, Message = {Message}, Data = {Data}", response.Id, response.Error.Code, response.Error.Message, response.Error.Data?.ToString());
        }
    }
}
