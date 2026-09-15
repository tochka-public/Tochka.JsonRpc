using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "custom")]
[Route("api/jsonrpc")]
public class CustomGroupNameJsonRpcController : JsonRpcControllerBase
{
    public bool CustomGroup() => true;

    public TestObject TestObjectTypes() => new();
}
