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
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Tests.Helpers;

namespace Tochka.JsonRpc.Common.Tests.Converters
{
    public class RequestWrapperConverterTests
    {
        private RequestWrapperConverter requestWrapperConverter;

        [SetUp]
        public void Setup()
        {
            requestWrapperConverter = new RequestWrapperConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            Action action = () => requestWrapperConverter.WriteJson(Mock.Of<JsonWriter>(), Mock.Of<IRequestWrapper>(), Mock.Of<JsonSerializer>());

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_ReadJson_ReturnsSingleForObject()
        {
            var json = new JObject().ToString();
            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new MockCallConverter()
                }
            });

            var result = requestWrapperConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, jsonSerializer);

            result.Should().BeOfType<SingleRequestWrapper>()
                .Subject.Call.Should().NotBeNull();
        }

        [Test]
        public void Test_ReadJson_ReturnsBatchForArray()
        {
            var jArray = new JArray
            {
                new JObject()
            };
            var json = jArray.ToString();
            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new MockCallConverter()
                }
            });

            var result = requestWrapperConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, jsonSerializer);

            result.Should().BeOfType<BatchRequestWrapper>();
            var batch = result as BatchRequestWrapper;
            batch.Batch.Should().HaveCount(1);
            batch.Batch[0].Should().Be(Mock.Get(batch.Batch[0]).Object);
        }

        [TestCaseSource(typeof(RequestWrapperConverterTests), nameof(BadJsonCases))]
        public void Test_ReadJson_ThrowsOnBadJson(string json)
        {
            var jsonSerializer = JsonSerializer.Create();
            Action action = () => requestWrapperConverter.ReadJson(CreateJsonReader(json), typeof(IRpcId), null, jsonSerializer);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static IEnumerable BadJsonCases => BadJson.Select(data => new TestCaseData(data));

        private static IEnumerable<string> BadJson
        {
            get
            {
                // Other possible values from ECMA-404, Section 5:
                yield return "null";
                yield return @"""""";
                yield return @"""test""";
                yield return "0";
                yield return "0.1";
                yield return "true";
                yield return "false";
            }
        }

        private static JsonTextReader CreateJsonReader(string json) => new JsonTextReader(new StringReader(json));
    }
}