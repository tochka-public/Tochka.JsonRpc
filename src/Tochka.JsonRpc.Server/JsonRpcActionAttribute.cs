namespace Tochka.JsonRpc.Server;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class JsonRpcActionAttribute : Attribute
{
    public JsonRpcActionAttribute(string method)
    {
        Method = method;
    }

    public string Method { get; }
}
