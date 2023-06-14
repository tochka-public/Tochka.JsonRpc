using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server;

public static class Utils
{
    public static JsonSerializerOptions GetDataJsonSerializerOptions(IEnumerable<object> endpointMetadata, JsonRpcServerOptions serverOptions, IEnumerable<IJsonSerializerOptionsProvider> providers)
    {
        var serializerOptionsAttribute = endpointMetadata.Get<JsonRpcSerializerOptionsAttribute>();
        return serializerOptionsAttribute == null
            ? serverOptions.DefaultDataJsonSerializerOptions
            : GetJsonSerializerOptions(providers, serializerOptionsAttribute.ProviderType);
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
