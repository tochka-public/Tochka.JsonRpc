using System.Text.Json;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Client.Tests.WebApplication;

public interface IResponseProvider
{
    JsonDocument GetResponse();
    TestData GetJsonRpcResponse();
}
