using Microsoft.AspNetCore.Mvc;

namespace RoutingTests;

[Route("/LOL")]
public class RestController : ControllerBase
{
    [Route("/lol")]
    public string Get() => "Hello World!";
}
