using System.Text.Json;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.Server.Serialization;

/// <summary>
/// Provider for <see cref="JsonSerializerOptions" /> to use by it's type in <see cref="JsonRpcSerializerOptionsAttribute" />
/// </summary>
public interface IJsonSerializerOptionsProvider
{
    /// <summary>
    /// Provided <see cref="JsonSerializerOptions" />
    /// </summary>
    JsonSerializerOptions Options { get; }
}
