using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common;

/// <inheritdoc />
/// <summary>
/// Exception indicating error in format of JSON-RPC object
/// </summary>
[PublicAPI]
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
