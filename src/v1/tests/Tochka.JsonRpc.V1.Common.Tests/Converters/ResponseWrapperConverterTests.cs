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
using Tochka.JsonRpc.V1.Common.Converters;
using Tochka.JsonRpc.V1.Common.Models.Id;
using Tochka.JsonRpc.V1.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.V1.Common.Tests.Helpers;

namespace Tochka.JsonRpc.V1.Common.Tests.Converters
{
    public class ResponseWrapperConverterTests
    {
        private ResponseWrapperConverter responseWrapperConverter;

        [SetUp]
        public void Setup()
        {
            responseWrapperConverter = new ResponseWrapperConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            Action action = () => responseWrapperConverter.WriteJson(Mock.Of<JsonWriter>(), Mock.Of<IResponseWrapper>(), Mock.Of<JsonSerializer>());

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
                    new MockResponseConverter()
                }
            });

            var result = responseWrapperConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, jsonSerializer);

            result.Should().BeOfType<SingleResponseWrapper>()
                .Subject.Single.Should().NotBeNull();
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
                    new MockResponseConverter()
                }
            });

            var result = responseWrapperConverter.ReadJson(CreateJsonReader(json), Mock.Of<Type>(), null, false, jsonSerializer);

            result.Should().BeOfType<BatchResponseWrapper>();
            var batch = result as BatchResponseWrapper;
            batch.Batch.Should().HaveCount(1);
            batch.Batch[0].Should().Be(Mock.Get(batch.Batch[0]).Object);
        }

        [TestCaseSource(typeof(ResponseWrapperConverterTests), nameof(BadJsonCases))]
        public void Test_ReadJson_ThrowsOnBadJson(string json)
        {
            var jsonSerializer = JsonSerializer.Create();
            Action action = () => responseWrapperConverter.ReadJson(CreateJsonReader(json), typeof(IRpcId), null, jsonSerializer);

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
