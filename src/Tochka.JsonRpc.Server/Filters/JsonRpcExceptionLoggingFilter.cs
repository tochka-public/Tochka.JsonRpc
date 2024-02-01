using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Filters;

/// <inheritdoc />
/// <summary>
/// Filter for JSON-RPC actions to log exceptions
/// </summary>
internal class JsonRpcExceptionLoggingFilter : IExceptionFilter
{
    private readonly JsonRpcServerOptions options;
    private readonly ILogger<JsonRpcExceptionLoggingFilter> log;

    public JsonRpcExceptionLoggingFilter(IOptions<JsonRpcServerOptions> options, ILogger<JsonRpcExceptionLoggingFilter> log)
    {
        this.options = options.Value;
        this.log = log;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.HttpContext.GetJsonRpcCall() == null)
        {
            return;
        }

        if (options.LogExceptions)
        {
            log.LogError(context.Exception, "Exception during JSON-RPC call processing");
        }
    }
}
