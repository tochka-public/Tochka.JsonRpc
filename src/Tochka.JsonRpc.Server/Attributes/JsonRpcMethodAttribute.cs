using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method)]
public sealed class JsonRpcMethodAttribute : Attribute
{
    public string Method { get; }

    public JsonRpcMethodAttribute(string method) => Method = method;
}
