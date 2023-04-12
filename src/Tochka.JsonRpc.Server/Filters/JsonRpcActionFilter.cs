using Microsoft.AspNetCore.Mvc.Filters;

namespace Tochka.JsonRpc.Server.Filters;

internal class JsonRpcActionFilter : IActionFilter
{
    // if model binding failed - is it really needed?
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
