using Asp.Versioning;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.TemplateRoutingApplication.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiVersion("3-str")]
public class VersionedController : JsonRpcControllerBase
{
    public bool Check() => true;
}
