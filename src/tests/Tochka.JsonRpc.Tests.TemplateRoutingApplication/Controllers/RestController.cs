using Microsoft.AspNetCore.Mvc;

namespace Tochka.JsonRpc.Tests.TemplateRoutingApplication.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class RestController : ControllerBase
{
    [HttpGet("check")]
    public bool Check() => true;
}
