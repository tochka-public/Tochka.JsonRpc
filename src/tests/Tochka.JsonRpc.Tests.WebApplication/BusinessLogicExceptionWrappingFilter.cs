using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Tests.WebApplication;

internal class BusinessLogicExceptionWrappingFilter : IExceptionFilter
{
    private readonly IJsonRpcErrorFactory errorFactory;

    public BusinessLogicExceptionWrappingFilter(IJsonRpcErrorFactory errorFactory) => this.errorFactory = errorFactory;

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not BusinessLogicException)
        {
            return;
        }

        var error = errorFactory.InternalError(ErrorData);
        context.Result = new ObjectResult(error);
    }

    public const string ErrorData = "handled with custom filter";
}
