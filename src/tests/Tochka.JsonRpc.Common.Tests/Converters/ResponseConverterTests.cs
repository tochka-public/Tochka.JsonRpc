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
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Common.Tests.Converters
{
    public class ResponseConverterTests
    {
        private ResponseConverter responseConverter;

        [SetUp]
        public void Setup()
        {
            responseConverter = new ResponseConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            Action action = () => responseConverter.WriteJson(Mock.Of<JsonWriter>(), Mock.Of<object>(), Mock.Of<JsonSerializer>());

            action.Should().Throw<InvalidOperationException>();
        }

        [TestCase(typeof(IResponse), true)]
        [TestCase(typeof(object), false)]
        public void Test_CanConvert_ChecksType(Type type, bool expected)
        {
            var result = responseConverter.CanConvert(type);

            result.Should().Be(expected);
        }

        [TestCase(@"{}")]
        [TestCase(@"{""id"":null}")]
        [TestCase(@"{""id"":null, ""result"": 1, ""error"": 2}")]
        [TestCase(@"{""id"":null, ""result"": null, ""error"": null}")]
        public void Test_ReadJson_ThrowsOnWrongProperties(string json)
        {
            Action action = () => responseConverter.ReadJson(CreateJsonReader(json), typeof(IResponse), null, new JsonSerializer());

            action.Should().Throw<ArgumentException>();
        }

        [TestCaseSource(typeof(ResponseConverterTests), nameof(JsonCases))]
        public void Test_ReadJson_ChecksResponseAndErrorProperties(string json, IResponse expected)
        {
            var result = responseConverter.ReadJson(CreateJsonReader(json), typeof(IResponse), null, new JsonSerializer());

            result.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable JsonCases => JsonResponses.Select(data => new TestCaseData(data.json, data.expected));

        private static IEnumerable<(string json, IResponse expected)> JsonResponses
        {
            get
            {
                yield return (
                    @"{""id"":null, ""result"": 2}",
                    new UntypedResponse()
                    {
                        Result = new JValue(2)
                    }
                );

                yield return (
                    @"{""id"":null, ""result"": null}",
                    new UntypedErrorResponse()
                );

                yield return (
                    @"{""id"":null, ""error"": {""code"": 1}}",
                    new UntypedErrorResponse()
                    {
                        Error = new Error<JToken>()
                        {
                            Code = 1
                        }
                    }
                );

                yield return (
                    @"{""id"":null, ""error"": null}",
                    new UntypedErrorResponse()
                );
            }
        }

        private static JsonTextReader CreateJsonReader(string json) => new JsonTextReader(new StringReader(json));
    }
}