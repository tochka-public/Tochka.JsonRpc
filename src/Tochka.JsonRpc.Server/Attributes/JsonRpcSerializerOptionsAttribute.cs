using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.Server.Attributes;

/// <inheritdoc />
/// <summary>
/// Attribute to override default data serializer options
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class JsonRpcSerializerOptionsAttribute : Attribute
{
    /// <summary>
    /// Type of data serializer options provider
    /// </summary>
    /// <remarks>
    /// Must implement <see cref="IJsonSerializerOptionsProvider" />
    /// </remarks>
    public Type ProviderType { get; }

    /// <inheritdoc />
    /// <param name="providerType">Type of data serializer options provider. Must implement <see cref="IJsonSerializerOptionsProvider" /></param>
    /// <exception cref="ArgumentException">If <paramref name="providerType" /> doesn't implement <see cref="IJsonSerializerOptionsProvider" /></exception>
    public JsonRpcSerializerOptionsAttribute(Type providerType)
    {
        if (!typeof(IJsonSerializerOptionsProvider).IsAssignableFrom(providerType))
        {
            throw new ArgumentException($"Expected implementation of {nameof(IJsonSerializerOptionsProvider)}", nameof(providerType));
        }

        ProviderType = providerType;
    }
}
