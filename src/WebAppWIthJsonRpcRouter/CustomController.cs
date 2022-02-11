namespace WebAppWIthJsonRpcRouter;

public class CustomController
{
    public ValueTask<string> Index()
    {
        return ValueTask.FromResult("alo");
    }
}