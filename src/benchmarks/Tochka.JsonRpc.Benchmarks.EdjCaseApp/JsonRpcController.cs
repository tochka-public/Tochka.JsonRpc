using EdjCase.JsonRpc.Router;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.EdjCaseApp;

[RpcRoute("api/jsonrpc")]
public class JsonRpcController : RpcController
{
    public TestData Process(TestData data) => data;
}
