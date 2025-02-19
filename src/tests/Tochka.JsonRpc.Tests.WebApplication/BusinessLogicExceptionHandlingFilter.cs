using Microsoft.AspNetCore.Mvc.Filters;

namespace Tochka.JsonRpc.Tests.WebApplication;

internal class BusinessLogicExceptionHandlingFilter : IExceptionFilter
{
    private readonly IBusinessLogicExceptionHandler handler;

    public BusinessLogicExceptionHandlingFilter(IBusinessLogicExceptionHandler handler) => this.handler = handler;

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is BusinessLogicException exception)
        {
            handler.Handle(exception);
        }
    }
}
