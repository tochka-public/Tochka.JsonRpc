using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common.Models.Request;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.V1.Common.Converters
{
    /// <summary>
    /// Handle dumb rule of Id present for requests and not present for notifications
    /// </summary>
    public class CallConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // NOTE: used in server to parse requests, no need for serialization
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var idProperty = jObject[JsonRpcConstants.IdProperty];
            var idType = idProperty?.Type;
            switch (idType)
            {
                case JTokenType.String:
                case JTokenType.Integer:
                case JTokenType.Null:
                    var idValue = idProperty as JValue;
                    var result1 = jObject.ToObject<UntypedRequest>(serializer);
                    result1.RawJson = jObject.ToString();
                    result1.RawId = idValue;
                    return result1;

                case null:
                    var result2 = jObject.ToObject<UntypedNotification>(serializer);
                    result2.RawJson = jObject.ToString();
                    return result2;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, "Expected string, number, null or nothing as Id");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ICall<>)
                   || objectType == typeof(IUntypedCall)
                ;
        }
    }
}
