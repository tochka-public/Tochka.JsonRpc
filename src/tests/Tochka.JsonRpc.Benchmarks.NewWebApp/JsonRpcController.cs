using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.Benchmarks.NewWebApp;

public class JsonRpcController : JsonRpcControllerBase
{
    public TestData Process(TestData data) => data;
}
