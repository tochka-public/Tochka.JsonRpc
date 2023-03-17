/*
using System;
using System.IO;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common.Converters;
using Tochka.JsonRpc.V1.Common.Models.Response;

namespace Tochka.JsonRpc.V1.Common.Tests.Converters
{
    public class ErrorConverterTests
    {
        private ErrorConverter errorConverter;

        [SetUp]
        public void Setup()
        {
            errorConverter = new ErrorConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            Action action = () => errorConverter.WriteJson(Mock.Of<JsonWriter>(), Mock.Of<object>(), Mock.Of<JsonSerializer>());

            action.Should().Throw<InvalidOperationException>();
        }

        [TestCase(typeof(IError), true)]
        [TestCase(typeof(UntypedError), false)]
        [TestCase(typeof(Error<>), false)]
        [TestCase(typeof(object), false)]
        public void Test_CanConvert_ChecksType(Type type, bool expected)
        {
            var result = errorConverter.CanConvert(type);

            result.Should().Be(expected);
        }

        [Test]
        public void Test_ReadJson_ReturnsUntypedError()
        {
            var json = @"{""code"": 1, ""message"": ""test"", ""data"": [], }";

            var result = errorConverter.ReadJson(CreateJsonReader(json), typeof(IError), null, JsonSerializer.Create());

            var expected = new UntypedError()
            {
                Data = new JArray(),
                Message = "test",
                Code = 1
            };
            result.Should().BeOfType<UntypedError>();
            result.Should().BeEquivalentTo(expected);
        }

        private static JsonTextReader CreateJsonReader(string json) => new JsonTextReader(new StringReader(json));
    }
}
*/