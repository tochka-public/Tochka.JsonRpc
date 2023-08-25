using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Server.Exceptions;

/// <inheritdoc />
/// <summary>
/// Exception thrown by JSON-RPC call processing logic
/// </summary>
[PublicAPI]
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
