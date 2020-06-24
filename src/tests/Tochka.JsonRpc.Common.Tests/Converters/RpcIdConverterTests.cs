using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Converters;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Tests.Converters
{
    public class RpcIdConverterTests
    {
        private RpcIdConverter rpcIdConverter;

        [SetUp]
        public void Setup()
        {
            rpcIdConverter = new RpcIdConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            Action action = () => rpcIdConverter.WriteJson(Mock.Of<JsonWriter>(), Mock.Of<IRpcId>(), Mock.Of<JsonSerializer>());

            action.Should().Throw<InvalidOperationException>();
        }

        [TestCase("")]
        [TestCase("test")]
        public void Test_ReadJson_ReturnsStringIdForString(string value)
        {
            var json = $@"""{value}""";

            var result = rpcIdConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, new JsonSerializer());

            result.Should().BeOfType<StringRpcId>()
                .Subject.String.Should().Be(value);
        }

        [TestCase(0)]
        [TestCase(42)]
        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        public void Test_ReadJson_ReturnsNumberIdForNumber(int value)
        {
            var json = new JValue(value).ToString();

            var result = rpcIdConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, new JsonSerializer());

            result.Should().BeOfType<NumberRpcId>()
                .Subject.Number.Should().Be(value);
        }

        [TestCase(0L)]
        [TestCase(42L)]
        [TestCase(-1L)]
        [TestCase(long.MaxValue)]
        public void Test_ReadJson_ReturnsNumberIdForNumber(long value)
        {
            var json = new JValue(value).ToString();

            var result = rpcIdConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, new JsonSerializer());

            result.Should().BeOfType<NumberRpcId>()
                .Subject.Number.Should().Be(value);
        }

        [Test]
        public void Test_ReadJson_ReturnsNullForNull()
        {
            var json = "null";

            var result = rpcIdConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, new JsonSerializer());

            result.Should().BeNull();
        }

        [TestCaseSource(typeof(RpcIdConverterTests), nameof(BadJsonIdCases))]
        public void Test_ReadJson_ThrowsOnBadIdProperty(string json)
        {
            var jsonSerializer = JsonSerializer.Create();
            Action action = () => rpcIdConverter.ReadJson(CreateJsonReader(json), typeof(IRpcId), null, jsonSerializer);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }
        
        private static IEnumerable BadJsonIdCases => BadJsonIds.Select(data => new TestCaseData(data));

        private static IEnumerable<string> BadJsonIds
        {
            get
            {
                // Other possible values from ECMA-404, Section 5:
                yield return "{}";
                yield return "[]";
                yield return "0.1";
                yield return "true";
                yield return "false";
            }
        }

        private static JsonTextReader CreateJsonReader(string json) => new JsonTextReader(new StringReader(json));
    }
}