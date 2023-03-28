using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

[ExcludeFromCodeCoverage]
public record UntypedNotification(string Method, JsonDocument? Params, string Jsonrpc = JsonRpcConstants.Version)
    : Notification<JsonDocument>(Method, Params, Jsonrpc), IUntypedCall;
