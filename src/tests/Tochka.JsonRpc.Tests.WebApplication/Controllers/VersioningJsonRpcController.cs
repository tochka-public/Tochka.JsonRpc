using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

[ApiVersion("1.0")]
public class VersioningJsonRpcController : JsonRpcControllerBase
{
    public bool ProcessVersioned() => true;
}

[ApiVersion("2.0")]
[Route("/v{version:apiVersion}")]
public class Versioning2JsonRpcController : JsonRpcControllerBase
{
    public bool ProcessVersioned() => false;
}
