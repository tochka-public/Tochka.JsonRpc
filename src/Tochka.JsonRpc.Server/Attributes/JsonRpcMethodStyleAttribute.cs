using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Attributes;

/// <inheritdoc />
/// <summary>
/// Attribute to override default JSON-RPC method style on controller/action
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcMethodStyleAttribute : Attribute
{
    /// <summary>
    /// Method style for controller/action
    /// </summary>
    public JsonRpcMethodStyle MethodStyle { get; }

    /// <inheritdoc />
    /// <param name="methodStyle">Method style for controller/action</param>
    public JsonRpcMethodStyleAttribute(JsonRpcMethodStyle methodStyle) => MethodStyle = methodStyle;
}
