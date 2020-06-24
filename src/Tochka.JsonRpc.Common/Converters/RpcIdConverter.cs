using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;

namespace Tochka.JsonRpc.Common.Converters
{
    /// <summary>
    /// Handle dumb rule of Id as string/number/null for requests
    /// </summary>
    public class RpcIdConverter : JsonConverter<IRpcId>
    {
        public override void WriteJson(JsonWriter writer, IRpcId value, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }

        public override IRpcId ReadJson(JsonReader reader, Type objectType, IRpcId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var idProperty = JToken.Load(reader);
            var idType = idProperty.Type;
            switch (idType)
            {
                case JTokenType.String:
                    return new StringRpcId(idProperty.Value<string>());

                case JTokenType.Integer:
                    return new NumberRpcId(idProperty.Value<long>());

                case JTokenType.Null:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, "Expected string, number or null as Id");
            }
        }
    }
}