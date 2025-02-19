using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

/// <inheritdoc cref="Notification{TParams}" />
/// <summary>
/// Notification with params as JsonDocument
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record UntypedNotification
(
    string Method,
    JsonDocument? Params,
    string Jsonrpc = JsonRpcConstants.Version
)
    : Notification<JsonDocument>(Method, Params, Jsonrpc),
        IUntypedCall;
