using Microsoft.AspNetCore.Mvc;

namespace WebAppWIthJsonRpcRouter;

public class TestController : Controller
{
    public Task<string> Index()
    {
        return Task.FromResult("Hello epta!");
    }
}