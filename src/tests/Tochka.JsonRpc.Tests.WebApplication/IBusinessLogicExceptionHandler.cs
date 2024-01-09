namespace Tochka.JsonRpc.Tests.WebApplication;

internal interface IBusinessLogicExceptionHandler
{
    void Handle(BusinessLogicException exception);
}
