using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

public class ExceptionsJsonRpcController : JsonRpcControllerBase
{
    public string BusinessLogicException() => throw new BusinessLogicException();

    public string UnexpectedException() => throw new InvalidOperationException();
}
