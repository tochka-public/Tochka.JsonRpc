using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped;

[ExcludeFromCodeCoverage]
public record UntypedRequest(IRpcId Id, string Method, JsonDocument? Params, string Jsonrpc = JsonRpcConstants.Version)
    : Request<JsonDocument>(Id, Method, Params, Jsonrpc), IUntypedCall;
