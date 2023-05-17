using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.ApiExplorer;

/// <inheritdoc />
/// <summary>
/// Pass method information to request/response types for metadata generation
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcTypeMetadataAttribute : Attribute
{
    public Type? SerializerOptionsProviderType { get; }
    public string MethodName { get; }

    public JsonRpcTypeMetadataAttribute(Type? serializerOptionsProviderType, string methodName)
    {
        SerializerOptionsProviderType = serializerOptionsProviderType;
        MethodName = methodName;
    }
}
