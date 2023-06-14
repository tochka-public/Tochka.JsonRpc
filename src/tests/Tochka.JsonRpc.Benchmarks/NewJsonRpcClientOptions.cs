using Tochka.JsonRpc.Client.Settings;

namespace Tochka.JsonRpc.Benchmarks;

public class NewJsonRpcClientOptions : JsonRpcClientOptionsBase
{
    public override string Url { get; init; } = Constants.BaseUrl;
}
