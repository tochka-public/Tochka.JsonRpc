using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

[ApiExplorerSettings(GroupName = "custom")]
public class CustomGroupNameJsonRpcController : JsonRpcControllerBase
{
    public bool CustomGroup() => true;

    public TestObject TestObjectTypes() => new();
}
