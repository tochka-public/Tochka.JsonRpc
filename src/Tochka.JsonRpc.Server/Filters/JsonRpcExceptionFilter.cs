using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Filters;

/// <inheritdoc />
/// <summary>
/// Filter for JSON-RPC actions to log exceptions and convert them to JSON-RPC error
/// </summary>
internal class JsonRpcExceptionFilter : IExceptionFilter
{
    private readonly IJsonRpcErrorFactory errorFactory;
    private readonly JsonRpcServerOptions options;
    private readonly ILogger<JsonRpcExceptionFilter> log;

    public JsonRpcExceptionFilter(IJsonRpcErrorFactory errorFactory, IOptions<JsonRpcServerOptions> options, ILogger<JsonRpcExceptionFilter> log)
    {
        this.errorFactory = errorFactory;
        this.options = options.Value;
        this.log = log;
    }

    // wrap exception in json rpc error
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

        var error = errorFactory.Exception(context.Exception);
        context.Result = new ObjectResult(error);
    }
}
