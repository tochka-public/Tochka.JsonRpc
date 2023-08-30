using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.TemplateRoutingApplication.Controllers;

public class UnversionedController : JsonRpcControllerBase
{
    public bool Check() => true;
}
