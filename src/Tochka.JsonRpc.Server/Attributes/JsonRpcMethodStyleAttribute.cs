using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Attributes;

/// <summary>
/// Override matching rule for JSON Rpc "method"
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcMethodStyleAttribute : Attribute
{
    public MethodStyle MethodStyle { get; }

    public JsonRpcMethodStyleAttribute(MethodStyle methodStyle)
    {
        MethodStyle = methodStyle;
    }
}
