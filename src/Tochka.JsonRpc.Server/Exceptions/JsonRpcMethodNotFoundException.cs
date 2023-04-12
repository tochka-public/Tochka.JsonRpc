using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Exceptions;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Method is required")]
public class JsonRpcMethodNotFoundException : Exception
{
    public string Method { get; }

    public JsonRpcMethodNotFoundException(string method) => Method = method;
}
