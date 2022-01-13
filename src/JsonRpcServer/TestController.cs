using Tochka.JsonRpc.Server.Pipeline;

namespace JsonRpcServer;

public class TestController : JsonRpcController
{
    public Task<string> ToLower(string? text)
    {
        return Task.FromResult(text!.ToLower());
    }
}