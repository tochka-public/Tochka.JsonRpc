using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Tochka.JsonRpc.V1.Common.Converters;

namespace Tochka.JsonRpc.V1.Common.Serializers
{
    [ExcludeFromCodeCoverage]
    public class HeaderJsonRpcSerializer : IJsonRpcSerializer
    {
        public JsonSerializerSettings Settings => SettingsInstance;
        public JsonSerializer Serializer => SerializerInstance;

        private static readonly JsonSerializerSettings SettingsInstance = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            },
            Converters = new List<JsonConverter>
            {
                // requests
                new RequestWrapperConverter(),
                new CallConverter(),
                new JsonRpcIdConverter(),

                // responses
                new ResponseWrapperConverter(),
                new ResponseConverter(),
            },
            Formatting = Formatting.Indented,
        };

        private static readonly JsonSerializer SerializerInstance = JsonSerializer.Create(SettingsInstance);
    }
}
