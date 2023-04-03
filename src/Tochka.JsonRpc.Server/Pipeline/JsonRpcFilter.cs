using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Server.Pipeline;

public class JsonRpcFilter : IAlwaysRunResultFilter, IActionFilter
{
    private readonly IActionResultConverter actionResultConverter;
    private readonly ILogger log;

    public JsonRpcFilter(IActionResultConverter actionResultConverter, ILogger<JsonRpcFilter> log)
    {
        this.actionResultConverter = actionResultConverter;
        this.log = log;
    }

    /// <inheritdoc />
    /// <summary>
    /// Wrap plain responses into rpc response. Does not get called on exceptions
    /// </summary>
    /// <param name="context"></param>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        log.LogTrace("{action} Started", nameof(OnResultExecuting));
        var methodMetadata = context.ActionDescriptor.GetProperty<MethodMetadata>() ?? throw new ArgumentNullException(nameof(MethodMetadata));
        var serializer = context.HttpContext.RequestServices.GetRequiredService(methodMetadata.MethodOptions.RequestSerializer) as IJsonRpcSerializer;
        context.Result = actionResultConverter.ConvertActionResult(context.Result, methodMetadata, serializer);
        context.HttpContext.Items[JsonRpcConstants.ActionResultTypeItemKey] = context.Result.GetType();
        context.HttpContext.Items[JsonRpcConstants.ActionDescriptorItemKey] = context.ActionDescriptor;
        log.LogTrace("{action} Completed", nameof(OnResultExecuting));
    }

    /// <inheritdoc />
    /// <summary>
    /// Return 400 if binding failed instead of passing bad model to action, similar to ApiControllerAttribute
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        log.LogTrace("{action} Started", nameof(OnActionExecuting));
        if (context.Result != null || context.ModelState.IsValid)
        {
            log.LogTrace("{action} Completed: result is not null: {resultIsNotNull}, model state is valid: {modelStateIsValid}",
                nameof(OnActionExecuting),
                context.Result != null,
                context.ModelState.IsValid);
            return;
        }

        var result = actionResultConverter.GetFailedBindingResult(context.ModelState);
        context.Result = result;
        context.HttpContext.Items[JsonRpcConstants.ActionResultTypeItemKey] = result.GetType();
        context.HttpContext.Items[JsonRpcConstants.ActionDescriptorItemKey] = context.ActionDescriptor;
        log.LogTrace("{action} Completed", nameof(OnActionExecuting));
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}