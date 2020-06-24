using System;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Converters;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Common.Tests.Helpers
{
    public class MockResponseConverter : ResponseConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken.Load(reader);
            return Mock.Of<IResponse>();
        }
    }
}