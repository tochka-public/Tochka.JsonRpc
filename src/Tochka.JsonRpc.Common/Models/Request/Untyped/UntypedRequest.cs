using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

[ExcludeFromCodeCoverage]
public sealed record UntypedRequest(IRpcId Id, string Method, JsonDocument? Params, string Jsonrpc = JsonRpcConstants.Version)
    : Request<JsonDocument>(Id, Method, Params, Jsonrpc), IUntypedCall;
