using Microsoft.AspNetCore.Mvc;

namespace RoutingTests;

[Route("[controller]")]
public class RestController : ControllerBase
{
    [HttpGet]
    [Route("/api")]
    [Route("/api/check")]
    public string Get() => "Hello World!";
}
