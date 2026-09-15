using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

[ApiController]
[Authorize]
[Route("/jsonrpc")]
public class AuthJsonRpcController : JsonRpcControllerBase
{
    public bool WithAuth() => true;

    [AllowAnonymous]
    public bool WithoutAuth() => true;
}
