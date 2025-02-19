using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common;

/// <inheritdoc />
/// <summary>
/// Exception indicating error in format of JSON-RPC object
/// </summary>
[ExcludeFromCodeCoverage]
public class JsonRpcFormatException : Exception
{
    /// <inheritdoc />
    public JsonRpcFormatException()
    {
    }

    /// <inheritdoc />
    public JsonRpcFormatException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public JsonRpcFormatException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
