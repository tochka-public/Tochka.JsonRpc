using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Common.Models.Response.Untyped;

/// <inheritdoc />
/// <summary>
/// Error response with error.data as JsonDocument
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record UntypedErrorResponse
(
    IRpcId Id,
    Error<JsonDocument> Error,
    string Jsonrpc = JsonRpcConstants.Version
)
    : ErrorResponse<JsonDocument>(Id, Error, Jsonrpc);
