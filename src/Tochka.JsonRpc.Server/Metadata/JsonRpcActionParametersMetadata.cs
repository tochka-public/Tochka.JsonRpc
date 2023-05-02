namespace Tochka.JsonRpc.Server.Metadata;

public sealed class JsonRpcActionParametersMetadata
{
    public Dictionary<string, JsonRpcParameterMetadata> Parameters { get; } = new();
}
