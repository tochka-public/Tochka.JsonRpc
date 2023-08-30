using Asp.Versioning;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.TemplateRoutingApplication.Controllers;

[ApiVersion(2)]
public class NewVersionController : JsonRpcControllerBase
{
    public bool Check() => true;
}
