using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Common.Models.Response;

/// <inheritdoc />
/// <summary>
/// Error response with typed error.data
/// </summary>
/// <param name="Id">Identifier established by the Client</param>
/// <param name="Error">Error returned from server</param>
/// <param name="Jsonrpc">Version of the JSON-RPC protocol</param>
/// <typeparam name="TError">Type of error</typeparam>
[ExcludeFromCodeCoverage]
public record ErrorResponse<TError>
(
    IRpcId Id,
    Error<TError> Error,
    string Jsonrpc = JsonRpcConstants.Version
) : IResponse;
