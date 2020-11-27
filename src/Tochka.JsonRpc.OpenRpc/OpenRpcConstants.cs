using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Tochka.JsonRpc.OpenRpc
{
    public class OpenRpcConstants
    {
        public const string SpecVersion = "1.2.6";
        public const string DocumentTemplateParameterName = "documentName";
        public static readonly string DefaultDocumentPath = $"openrpc/{{{DocumentTemplateParameterName}}}.json";

        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
            },
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}