using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.NewWebApp;

public class JsonRpcController : JsonRpcControllerBase
{
    public TestData Process(TestData data) => data;
}
