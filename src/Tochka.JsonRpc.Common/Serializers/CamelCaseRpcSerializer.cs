using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Tochka.JsonRpc.Common.Converters;

namespace Tochka.JsonRpc.Common.Serializers
{
    [ExcludeFromCodeCoverage]
    public class CamelCaseRpcSerializer : IRpcSerializer
    {
        public JsonSerializerSettings Settings => SettingsInstance;
        public JsonSerializer Serializer => SerializerInstance;

        private static readonly JsonSerializerSettings SettingsInstance = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(typeof(CamelCaseNamingStrategy)),
            },
            Formatting = Formatting.Indented,
        };

        private static readonly JsonSerializer SerializerInstance = JsonSerializer.Create(SettingsInstance);
    }
}