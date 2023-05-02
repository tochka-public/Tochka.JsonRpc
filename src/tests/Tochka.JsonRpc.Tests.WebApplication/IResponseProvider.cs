using System.Text.Json;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Tests.WebApplication;

public interface IResponseProvider
{
    JsonDocument GetResponse();
    TestData GetJsonRpcResponse();
}
