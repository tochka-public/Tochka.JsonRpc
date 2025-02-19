using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Attributes;

/// <inheritdoc />
/// <summary>
/// Attribute to manually set JSON-RPC method name for mapping to action
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method)]
public sealed class JsonRpcMethodAttribute : Attribute
{
    /// <summary>
    /// JSON-RPC method name to map
    /// </summary>
    public string Method { get; }

    /// <inheritdoc />
    /// <param name="method">JSON-RPC method name to map</param>
    public JsonRpcMethodAttribute(string method) => Method = method;
}
