using Asp.Versioning;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.TemplateRoutingApplication.Controllers;

[ApiVersion(1)]
public class OldVersionController : JsonRpcControllerBase
{
    public bool Check() => true;
}
