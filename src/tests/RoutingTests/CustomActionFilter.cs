using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace RoutingTests;

internal class CustomActionFilter : IAlwaysRunResultFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context) => context.Result = new ObjectResult(true);

    public void OnException(ExceptionContext context) => context.Result = new ObjectResult(new Error<object>(456, "lol", null));

    public void OnResultExecuting(ResultExecutingContext context) => context.Result = new ObjectResult(true);

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
