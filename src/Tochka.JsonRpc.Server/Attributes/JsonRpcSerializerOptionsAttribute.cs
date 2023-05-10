using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.Server.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcSerializerOptionsAttribute : Attribute
{
    public Type ProviderType { get; }

    public JsonRpcSerializerOptionsAttribute(Type providerType)
    {
        if (!typeof(IJsonSerializerOptionsProvider).IsAssignableFrom(providerType))
        {
            throw new ArgumentException($"Expected implementation of {nameof(IJsonSerializerOptionsProvider)}", nameof(providerType));
        }

        ProviderType = providerType;
    }
}
