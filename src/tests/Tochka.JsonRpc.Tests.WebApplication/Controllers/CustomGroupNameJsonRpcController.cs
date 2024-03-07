using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

public class TestObject
{
    public TimeSpan Ts { get; set; } = DateTime.Now.TimeOfDay;
}

[ApiExplorerSettings(GroupName = "custom")]
public class CustomGroupNameJsonRpcController : JsonRpcControllerBase
{
    public bool CustomGroup() => true;
    
    public TestObject TestObjectTypes() => new();
}
