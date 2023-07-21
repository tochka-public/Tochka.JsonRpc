using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.V1.Common.Converters
{
    /// <summary>
    /// Handle dumb rule of request being single or batch
    /// </summary>
    public class RequestWrapperConverter : JsonConverter<IRequestWrapper>
    {
        public override void WriteJson(JsonWriter writer, IRequestWrapper value, JsonSerializer serializer)
        {
            // NOTE: used in server to parse requests, no need for serialization
            throw new InvalidOperationException();
        }

        public override IRequestWrapper ReadJson(JsonReader reader, Type objectType, IRequestWrapper existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var tokenType = token.Type;
            switch (tokenType)
            {
                case JTokenType.Object:
                    var request = token.ToObject<IUntypedCall>(serializer);
                    return new SingleRequestWrapper() {Call = request};

                case JTokenType.Array:
                    var batch = token.ToObject<List<IUntypedCall>>(serializer);
                    return new BatchRequestWrapper() {Batch = batch};

                default:
                    throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Expected {} or [] as root element");
            }
        }
    }
}
