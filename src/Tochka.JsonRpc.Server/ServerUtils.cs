using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server;

/// <summary>
/// Helpers for JSON-RPC server logic
/// </summary>
public static class ServerUtils
{
    /// <summary>
    /// Get data serializer options from endpoint metadata (or default if no there was no attribute)
    /// </summary>
    /// <param name="endpointMetadata">Endpoint metadata to search for <see cref="JsonRpcSerializerOptionsAttribute" /></param>
    /// <param name="serverOptions">JSON-RPC server options</param>
    /// <param name="providers">All <see cref="IJsonSerializerOptionsProvider" /> registered in DI</param>
    /// <exception cref="ArgumentException">If specified in attribute provider not in <paramref name="providers" /></exception>
    public static JsonSerializerOptions GetDataJsonSerializerOptions(IEnumerable<object> endpointMetadata, JsonRpcServerOptions serverOptions, IEnumerable<IJsonSerializerOptionsProvider> providers)
    {
        var serializerOptionsAttribute = endpointMetadata.Get<JsonRpcSerializerOptionsAttribute>();
        return serializerOptionsAttribute == null
            ? serverOptions.DefaultDataJsonSerializerOptions
            : GetJsonSerializerOptions(providers, serializerOptionsAttribute.ProviderType);
    }

    /// <summary>
    /// Get data serializer options by provider type
    /// </summary>
    /// <param name="providers">All <see cref="IJsonSerializerOptionsProvider" /> registered in DI</param>
    /// <param name="providerType">Type of provider to get</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If provider with <paramref name="providerType" /> not in <paramref name="providers" /></exception>
    public static JsonSerializerOptions GetJsonSerializerOptions(IEnumerable<IJsonSerializerOptionsProvider> providers, Type providerType)
    {
        var serializerOptionsProvider = providers.FirstOrDefault(p => p.GetType() == providerType);
        if (serializerOptionsProvider == null)
        {
            throw new ArgumentException($"{nameof(IJsonSerializerOptionsProvider)} with implementation type [{providerType}] not found. Maybe it is not registered in DI?");
        }

        return serializerOptionsProvider.Options;
    }
}
