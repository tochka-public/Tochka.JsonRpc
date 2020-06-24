using System;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Converters;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Tests.Helpers
{
    public class MockRpcIdConverter : RpcIdConverter
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