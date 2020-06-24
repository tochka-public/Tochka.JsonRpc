using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Common.Tests.Serializers
{
    public class DeserializationTests
    {
        private readonly HeaderRpcSerializer headerRpcSerializer = new HeaderRpcSerializer();

        [TestCaseSource(typeof(JsonResponses), nameof(JsonResponses.Cases))]
        [TestCaseSource(typeof(JsonErrorResponses), nameof(JsonErrorResponses.Cases))]
        public void Test_Deserialize_Response(string responseString, object expected)
        {
            var jToken = JToken.Parse(responseString);
            var result = jToken.ToObject(expected.GetType(), headerRpcSerializer.Serializer);
            result.Should().BeOfType(expected.GetType());
            result.Should().BeEquivalentTo(expected);
        }
    }
}