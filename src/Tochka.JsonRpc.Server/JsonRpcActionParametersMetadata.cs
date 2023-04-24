namespace Tochka.JsonRpc.Server;

internal class JsonRpcActionParametersMetadata
{
    public Dictionary<string, JsonRpcParameterMetadata> Parameters { get; } = new();
}
