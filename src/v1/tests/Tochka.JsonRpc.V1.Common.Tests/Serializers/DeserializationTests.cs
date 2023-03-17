using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Common.Tests.Serializers
{
    public class DeserializationTests
    {
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer = new HeaderJsonRpcSerializer();

        [TestCaseSource(typeof(JsonResponses), nameof(JsonResponses.Cases))]
        [TestCaseSource(typeof(JsonErrorResponses), nameof(JsonErrorResponses.Cases))]
        public void Test_Deserialize_Response(string responseString, object expected)
        {
            var jToken = JToken.Parse(responseString);
            var result = jToken.ToObject(expected.GetType(), headerJsonRpcSerializer.Serializer);
            result.Should().BeOfType(expected.GetType());
            result.Should().BeEquivalentTo(expected);
        }
    }
}
