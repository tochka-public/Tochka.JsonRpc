using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Server;

internal class JsonRpcFilter : IActionFilter, IExceptionFilter, IAlwaysRunResultFilter
{
    // if model binding failed - is it really needed?
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    // wrap exception in json rpc error
    public void OnException(ExceptionContext context)
    {
        if (context.HttpContext.GetJsonRpcCall() == null)
        {
            return;
        }

        context.Result = new ObjectResult(new Error<ExceptionInfo>(123, "123", new ExceptionInfo(context.Exception.GetType().Name, context.Exception.Message, null)));
    }

    // wrap action results in json rpc response
    public void OnResultExecuting(ResultExecutingContext context)
    {
        var call = context.HttpContext.GetJsonRpcCall();
        if (call is UntypedNotification)
        {
            context.Result = new EmptyResult();
            return;
        }

        if (call is not UntypedRequest request)
        {
            return;
        }

        context.Result = context.Result switch
        {
            ObjectResult { Value: IError error } => new ObjectResult(new UntypedErrorResponse(request.Id, new Error<JsonDocument>(error.Code, error.Message, JsonSerializer.SerializeToDocument(error.Data)))),
            ObjectResult result => new ObjectResult(new UntypedResponse(request.Id, JsonSerializer.SerializeToDocument(result.Value))),
            _ => context.Result
        };
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
