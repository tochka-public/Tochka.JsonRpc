using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Metadata;

/// <summary>
/// Metadata with information about all parameters of action
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class JsonRpcActionParametersMetadata
{
    /// <summary>
    /// Parameters metadata by their name
    /// </summary>
    public Dictionary<string, JsonRpcParameterMetadata> Parameters { get; } = new();
}
