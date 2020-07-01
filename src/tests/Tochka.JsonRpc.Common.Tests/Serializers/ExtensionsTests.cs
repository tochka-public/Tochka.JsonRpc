using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Common.Tests.Converters;

namespace Tochka.JsonRpc.Common.Tests.Serializers
{
    public class ExtensionsTests
    {
        [Test]
        public void Test_SerializeParams_WorksForObject()
        {
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());

            var result = serializerMock.Object.SerializeParams(new object());

            result.Should().BeOfType<JObject>();
        }

        [Test]
        public void Test_SerializeParams_WorksForArray()
        {
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());

            var result = serializerMock.Object.SerializeParams(new List<int>{1,2});

            result.Should().BeOfType<JArray>();
            result.Should().HaveCount(2);
        }

        [TestCaseSource(typeof(ExtensionsTests), nameof(BadCases))]
        public void Test_SerializeParams_ThrowsOnOtherTypes(object value)
        {
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            Action action = () => serializerMock.Object.SerializeParams(value);

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_SerializeParams_ReturnsNullOnNull()
        {
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());

            var result = serializerMock.Object.SerializeParams((string)null);

            result.Should().BeNull();
        }

        private static IEnumerable BadCases => BadValues.Select(data => new TestCaseData(data));

        private static IEnumerable<object> BadValues
        {
            get
            {
                yield return "";
                yield return "test";
                yield return 1;
                yield return (int?)1;
                yield return 1.1;
                yield return true;
            }
        }
    }
}