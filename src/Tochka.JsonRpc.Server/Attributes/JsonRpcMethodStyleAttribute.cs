using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcMethodStyleAttribute : Attribute
{
    public JsonRpcMethodStyle MethodStyle { get; }

    public JsonRpcMethodStyleAttribute(JsonRpcMethodStyle methodStyle) => MethodStyle = methodStyle;
}
