using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.OldWebApp;

public class JsonRpcController : V1.Server.Pipeline.JsonRpcController
{
    public TestData Process(TestData data) => data;
}
