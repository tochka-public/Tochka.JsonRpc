using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Common.Converters
{
    public class ResponseConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // NOTE: used in client to parse responses, no need for serialization
            throw new InvalidOperationException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var idProperty = jObject[JsonRpcConstants.IdProperty];
            var resultProperty = jObject[JsonRpcConstants.ResultProperty];
            var errorProperty = jObject[JsonRpcConstants.ErrorProperty];
            var hasResult = resultProperty != null;
            var hasError = errorProperty != null;
            if (idProperty == null)
            {
                // "This member is REQUIRED. If there was an error in detecting the id in the Request object (e.g. Parse error/Invalid Request), it MUST be Null."
                // (JToken.Null, not actual null)
                throw new ArgumentException($"JSON Rpc response does not have [{JsonRpcConstants.IdProperty}] property");
            }

            if (hasResult && !hasError)
            {
                // result: This member is REQUIRED on success. This member MUST NOT exist if there was an error invoking the method.
                var result = jObject.ToObject<UntypedResponse>(serializer);
                result.RawResult = resultProperty.ToString();
                result.RawId = idProperty as JValue;
                return result;
            }


            if (!hasResult && hasError)
            {
                // error: This member is REQUIRED on error. This member MUST NOT exist if there was no error triggered during invocation.
                var result = jObject.ToObject<UntypedErrorResponse>(serializer);
                result.RawError = errorProperty.ToString();
                result.RawId = idProperty as JValue;
                return result;
            }

            throw new ArgumentException($"JSON Rpc response is invalid, expected one of properties. Has [{JsonRpcConstants.ResultProperty}]: {hasResult}. Has [{JsonRpcConstants.ErrorProperty}]: {hasError}");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IResponse);
        }
    }
}