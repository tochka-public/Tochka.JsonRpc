using System.Text.Json;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server;

public static class Utils
{
    public static JsonSerializerOptions GetDataJsonSerializerOptions(IEnumerable<object> endpointMetadata, JsonRpcServerOptions serverOptions, IEnumerable<IJsonSerializerOptionsProvider> providers)
    {
        var serializerOptionsMetadata = endpointMetadata.FirstOrDefault(static m => m is JsonRpcSerializerOptionsAttribute);
        if (serializerOptionsMetadata is not JsonRpcSerializerOptionsAttribute serializerOptionsAttribute)
        {
            return serverOptions.DefaultDataJsonSerializerOptions;
        }

        return GetJsonSerializerOptions(providers, serializerOptionsAttribute.ProviderType);
    }

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
