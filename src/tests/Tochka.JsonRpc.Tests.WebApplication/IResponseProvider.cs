using System.Text.Json;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Tests.WebApplication;

public interface IResponseProvider
{
    JsonDocument GetResponse();
    TestData GetJsonRpcResponse();
}
