using Microsoft.AspNetCore.Authorization;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

[Authorize]
public class AuthController : JsonRpcControllerBase
{
    public bool WithAuth() => true;

    [AllowAnonymous]
    public bool WithoutAuth() => true;
}
