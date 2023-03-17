using System;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Common.Tests.Serializers
{
    public class SerializationTests
    {
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer = new HeaderJsonRpcSerializer();

        [TestCaseSource(typeof(JsonRequests), nameof(JsonRequests.Cases))]
        public void Test_Deserialize_Request(string requestString, object expected)
        {
            var jToken = JToken.Parse(requestString);
            var result = jToken.ToObject(expected.GetType(), headerJsonRpcSerializer.Serializer);
            result.Should().BeOfType(expected.GetType());
            result.Should().BeEquivalentTo(expected, options => 
                options
                    .Excluding(x => IgnoreRawJson(x))
                    .Excluding(x => IgnoreRawId(x)));
        }

        private static bool IgnoreRawId(IMemberInfo x)
        {
            return x.DeclaringType == typeof(UntypedRequest) && x.Name == nameof(UntypedRequest.RawId);
        }

        private static bool IgnoreRawJson(IMemberInfo x)
        {
            return x.DeclaringType.IsAssignableFrom(typeof(IUntypedCall)) && x.Name == nameof(IUntypedCall.RawJson);
        }
    }
}
