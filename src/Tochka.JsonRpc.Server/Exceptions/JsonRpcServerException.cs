using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Exceptions;

/// <inheritdoc />
/// <summary>
/// Exception thrown by JSON-RPC call processing logic
/// </summary>
[ExcludeFromCodeCoverage]
public class JsonRpcServerException : Exception
{
    /// <inheritdoc />
    public JsonRpcServerException()
    {
    }

    /// <inheritdoc />
    public JsonRpcServerException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public JsonRpcServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
