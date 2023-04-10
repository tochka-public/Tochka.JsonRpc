using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;

namespace RoutingTests;

public class JsonRpcController : JsonRpcControllerBase
{
    public string ProcessAnything() => "Hello World!";
}
