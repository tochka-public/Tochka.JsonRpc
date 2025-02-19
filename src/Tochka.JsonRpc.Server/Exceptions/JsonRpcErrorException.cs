using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Exceptions;

/// <inheritdoc />
/// <summary>
/// Exception with error that will be converted to JSON-RPC error response if thrown from controller methods
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Error is required")]
public class JsonRpcErrorException : Exception
{
    /// <summary>
    /// Error to return in response
    /// </summary>
    public IError Error { get; }

    /// <inheritdoc />
    /// <param name="error">Error to return in response</param>
    public JsonRpcErrorException(IError error) => Error = error;
}
