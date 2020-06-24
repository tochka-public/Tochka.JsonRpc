using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Tochka.JsonRpc.Common.Converters;

namespace Tochka.JsonRpc.Common.Serializers
{
    [ExcludeFromCodeCoverage]
    public class SnakeCaseRpcSerializer : IRpcSerializer
    {
        public JsonSerializerSettings Settings => SettingsInstance;
        public JsonSerializer Serializer => SerializerInstance;

        private static readonly JsonSerializerSettings SettingsInstance = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(typeof(SnakeCaseNamingStrategy)),
            },
            Formatting = Formatting.Indented,
        };

        private static readonly JsonSerializer SerializerInstance = JsonSerializer.Create(SettingsInstance);
    }
}