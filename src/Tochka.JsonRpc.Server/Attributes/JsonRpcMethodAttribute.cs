namespace Tochka.JsonRpc.Server.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class JsonRpcMethodAttribute : Attribute
{
    public string Method { get; }

    public JsonRpcMethodAttribute(string method) => Method = method;
}
