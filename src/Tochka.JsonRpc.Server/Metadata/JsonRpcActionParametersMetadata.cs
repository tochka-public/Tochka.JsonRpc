using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Metadata;

[ExcludeFromCodeCoverage]
public sealed class JsonRpcActionParametersMetadata
{
    public Dictionary<string, JsonRpcParameterMetadata> Parameters { get; } = new();
}
