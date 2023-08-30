using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Tests.TemplateRoutingApplication.Controllers;

[Route("/route")]
public class CustomRouteController : JsonRpcControllerBase
{
    public bool Check() => true;
}
