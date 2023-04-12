using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcMethodStyleAttribute : Attribute
{
    public JsonRpcMethodStyleAttribute(JsonRpcMethodStyle methodStyle) => MethodStyle = methodStyle;

    public JsonRpcMethodStyle MethodStyle { get; }
}
