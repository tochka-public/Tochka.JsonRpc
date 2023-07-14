using System.Text.Json;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

/// <inheritdoc />
/// <summary>
/// Base interface for calls with params as JsonDocument
/// </summary>
[PublicAPI]
public interface IUntypedCall : ICall<JsonDocument>
{
}
