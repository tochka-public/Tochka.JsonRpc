using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Server.Metadata;

/// <summary>
/// Metadata with information about all parameters of action
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed class JsonRpcActionParametersMetadata
{
    /// <summary>
    /// Parameters metadata by their name
    /// </summary>
    public Dictionary<string, JsonRpcParameterMetadata> Parameters { get; } = new();
}
