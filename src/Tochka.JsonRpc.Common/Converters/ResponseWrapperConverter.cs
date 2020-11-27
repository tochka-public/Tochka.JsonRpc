using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Common.Converters
{
    /// <summary>
    /// Handle dumb rule of response being single on some batch errors
    /// </summary>
    public class ResponseWrapperConverter : JsonConverter<IResponseWrapper>
    {
        public override void WriteJson(JsonWriter writer, IResponseWrapper value, JsonSerializer serializer)
        {
            // NOTE: used in client to parse responses, no need for serialization
            throw new InvalidOperationException();
        }

        public override IResponseWrapper ReadJson(JsonReader reader, Type objectType, IResponseWrapper existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var tokenType = token.Type;
            switch (tokenType)
            {
                case JTokenType.Object:
                    var request = token.ToObject<IResponse>(serializer);
                    return new SingleResponseWrapper() {Single = request};

                case JTokenType.Array:
                    var batch = token.ToObject<List<IResponse>>(serializer);
                    return new BatchResponseWrapper() {Batch = batch};

                default:
                    throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Expected {} or [] as root element");
            }
        }
    }
}