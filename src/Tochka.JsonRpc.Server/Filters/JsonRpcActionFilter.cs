using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tochka.JsonRpc.Server.Extensions;

namespace Tochka.JsonRpc.Server.Filters;

/// <inheritdoc />
/// <summary>
/// Filter for JSON-RPC actions to return <see cref="BadRequestObjectResult" /> if model binding failed
/// </summary>
internal class JsonRpcActionFilter : IActionFilter
{
    // if model binding failed
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.GetJsonRpcCall() == null)
        {
            return;
        }

        if (context.Result != null || context.ModelState.IsValid)
        {
            return;
        }

        context.Result = new BadRequestObjectResult(context.ModelState);
    }

    [ExcludeFromCodeCoverage]
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
