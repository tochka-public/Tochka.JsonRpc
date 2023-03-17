using System;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common.Converters;
using Tochka.JsonRpc.V1.Common.Models.Id;

namespace Tochka.JsonRpc.V1.Common.Tests.Helpers
{
    public class MockJsonRpcIdConverter : JsonRpcIdConverter
    {
        public override void WriteJson(JsonWriter writer, IRpcId value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IRpcId ReadJson(JsonReader reader, Type objectType, IRpcId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken.Load(reader);
            return Mock.Of<IRpcId>();
        }
    }
}
