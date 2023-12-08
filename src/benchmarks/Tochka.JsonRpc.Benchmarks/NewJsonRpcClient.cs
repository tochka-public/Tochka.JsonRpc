using Microsoft.Extensions.Logging;
using Moq;
using Tochka.JsonRpc.Client;
using Tochka.JsonRpc.Client.Services;

namespace Tochka.JsonRpc.Benchmarks;

public class NewJsonRpcClient : JsonRpcClientBase
{
    public NewJsonRpcClient(HttpClient client) : base(client, new JsonRpcIdGenerator(), Mock.Of<ILogger<NewJsonRpcClient>>()) =>
        client.BaseAddress = new Uri(Constants.BaseUrl);
}
