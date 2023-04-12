namespace Tochka.JsonRpc.Server.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class JsonRpcMethodAttribute : Attribute
{
    public JsonRpcMethodAttribute(string method) => Method = method;

    public string Method { get; }
}
