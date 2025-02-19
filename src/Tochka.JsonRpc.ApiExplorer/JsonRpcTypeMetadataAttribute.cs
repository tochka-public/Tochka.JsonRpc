using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.ApiExplorer;

/// <inheritdoc />
/// <summary>
/// Pass method information to request/response types for metadata generation
/// </summary>
/// <remarks>
/// Dont use this attribute manually! It's for autodoc generation only, will not affect anything
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcTypeMetadataAttribute : Attribute
{
    /// <summary>
    /// Type of data serializer options provider
    /// </summary>
    public Type? SerializerOptionsProviderType { get; }

    /// <summary>
    /// JSON-RPC method name
    /// </summary>
    public string MethodName { get; }

    /// <inheritdoc />
    /// <param name="serializerOptionsProviderType">Type of data serializer options provider</param>
    /// <param name="methodName">JSON-RPC method name</param>
    public JsonRpcTypeMetadataAttribute(Type? serializerOptionsProviderType, string methodName)
    {
        SerializerOptionsProviderType = serializerOptionsProviderType;
        MethodName = methodName;
    }
}
