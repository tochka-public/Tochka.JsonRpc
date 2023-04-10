using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace RoutingTests;

[Route("[controller]")]
public class JsonRpcController : JsonRpcControllerBase
{
    public string ProcessAnything() => throw new ArgumentException("Hello World!");
}
