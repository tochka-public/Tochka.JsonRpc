using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Models.Response.Untyped;

[ExcludeFromCodeCoverage]
public sealed record UntypedResponse(IRpcId Id, JsonDocument? Result, string Jsonrpc = JsonRpcConstants.Version)
    : Response<JsonDocument>(Id, Result, Jsonrpc);
