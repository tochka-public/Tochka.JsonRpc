using EdjCase.JsonRpc.Router;

namespace Tochka.JsonRpc.Benchmarks.EdjCaseApp;

[RpcRoute("api/jsonrpc")]
public class JsonRpcController : RpcController
{
    public TestData Process(TestData data) => data;
}
