using Tochka.JsonRpc.V1.Client.Settings;

namespace Tochka.JsonRpc.Client.Tests.Integration;

internal class SimpleJsonRpcClientOptions : JsonRpcClientOptionsBase
{
    public override string Url { get; set; } = "https://localhost/";
}
