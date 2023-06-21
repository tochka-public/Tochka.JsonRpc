using Tochka.JsonRpc.V1.Client.Settings;

namespace Tochka.JsonRpc.Benchmarks;

public class OldJsonRpcClientOptions : JsonRpcClientOptionsBase
{
    public override string Url { get; set; } = Constants.BaseUrl;
}
