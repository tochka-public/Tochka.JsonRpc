using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Models.Response;

/// <inheritdoc />
/// <summary>
/// Successful response with typed result
/// </summary>
/// <param name="Id">Identifier established by the Client</param>
/// <param name="Result">Result of the method invoked on the Server</param>
/// <param name="Jsonrpc">Version of the JSON-RPC protocol</param>
/// <typeparam name="TResult">Type of result</typeparam>
[ExcludeFromCodeCoverage]
public record Response<TResult>
(
    IRpcId Id,
    TResult? Result,
    string Jsonrpc = JsonRpcConstants.Version
) : IResponse
{
    // required for autodoc metadata generation
    internal Response() : this(null!)
    {
    }
}
