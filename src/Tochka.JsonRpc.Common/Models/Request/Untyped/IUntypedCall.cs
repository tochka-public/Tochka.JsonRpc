using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

/// <inheritdoc />
/// <summary>
/// Base interface for calls with params as JsonDocument
/// </summary>
public interface IUntypedCall : ICall<JsonDocument>
{
}
