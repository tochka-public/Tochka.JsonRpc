using Tochka.JsonRpc.Client.Settings;

namespace Tochka.JsonRpc.Client.Tests.Integration;

internal class SimpleJsonRpcClientOptions : JsonRpcClientOptionsBase
{
    public override string Url { get; init; } = "https://localhost/";
}
