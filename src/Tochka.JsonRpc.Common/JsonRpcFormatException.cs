using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common;

[ExcludeFromCodeCoverage]
public class JsonRpcFormatException : Exception
{
    public JsonRpcFormatException()
    {
    }

    public JsonRpcFormatException(string message) : base(message)
    {
    }

    public JsonRpcFormatException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
