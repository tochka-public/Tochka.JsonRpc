using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server.Pipeline;

namespace WebAppWIthJsonRpcRouter;

public class JsonController : JsonRpcController
{
    public Task<string> ToLower(string? text)
    {
        return Task.FromResult(text!.ToLower());
    }
}