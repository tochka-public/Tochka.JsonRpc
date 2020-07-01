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
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Tests.Helpers;

namespace Tochka.JsonRpc.Common.Tests.Converters
{
    public class CallConverterTests
    {
        private CallConverter callConverter;

        [SetUp]
        public void Setup()
        {
            callConverter = new CallConverter();
        }

        [Test]
        public void Test_WriteJson_Throws()
        {
            Action action = () => callConverter.WriteJson(Mock.Of<JsonWriter>(), Mock.Of<object>(), Mock.Of<JsonSerializer>());

            action.Should().Throw<InvalidOperationException>();
        }

        [TestCase(typeof(ICall<>), true)]
        [TestCase(typeof(IUntypedCall), true)]
        [TestCase(typeof(object), false)]
        public void Test_CanConvert_ChecksType(Type type, bool expected)
        {
            var result = callConverter.CanConvert(type);

            result.Should().Be(expected);
        }

        [TestCase(typeof(ICall<int>))]
        [TestCase(typeof(ICall<string>))]
        [TestCase(typeof(IUntypedCall))]
        public void Test_ReadJson_ReturnsNotificationOnNoProperty(Type targetType)
        {
            var json = new JObject().ToString();

            var result = callConverter.ReadJson(CreateJsonReader(json), targetType, null, new JsonSerializer());

            result.Should().BeOfType<UntypedNotification>()
                .Subject.RawJson.Should().Be(json);
        }

        [TestCase(typeof(ICall<int>))]
        [TestCase(typeof(ICall<string>))]
        [TestCase(typeof(IUntypedCall))]
        public void Test_ReadJson_ReturnsRequestOnIdProperty(Type targetType)
        {
            var json = new JObject
            {
                [JsonRpcConstants.IdProperty] = null
            }.ToString();

            var result = callConverter.ReadJson(CreateJsonReader(json), targetType, null, JsonSerializer.Create());

            result.Should().BeOfType<UntypedRequest>();
            var request = result as UntypedRequest;
            request.RawJson.Should().NotBeNullOrWhiteSpace();
            Assert.AreEqual(JValue.CreateNull(), request.RawId);
        }

        [TestCaseSource(typeof(CallConverterTests), nameof(JsonIdCases))]
        public void Test_ReadJson_ReturnsRequestOnIdProperty(JValue jsonId)
        {
            var json = new JObject
            {
                [JsonRpcConstants.IdProperty] = jsonId
            }.ToString();
            
            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new MockJsonRpcIdConverter()
                }
            });
            var result = callConverter.ReadJson(CreateJsonReader(json), typeof(UntypedRequest), null, jsonSerializer);

            result.Should().BeOfType<UntypedRequest>()
                .Subject.RawJson.Should().NotBeNullOrWhiteSpace();
            var request = result as UntypedRequest;
            request.RawId.Type.Should().Be(jsonId.Type);
            request.RawId.Value.Should().Be(jsonId.Value);
        }

        [TestCaseSource(typeof(CallConverterTests), nameof(BadJsonIdCases))]
        public void Test_ReadJson_ThrowsOnBadIdProperty(JToken jsonId)
        {
            var json = new JObject
            {
                [JsonRpcConstants.IdProperty] = jsonId
            }.ToString();

            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new MockJsonRpcIdConverter()
                }
            });
            Action action = () => callConverter.ReadJson(CreateJsonReader(json), typeof(UntypedRequest), null, jsonSerializer);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static IEnumerable JsonIdCases => JsonIds.Select(data => new TestCaseData(data));

        private static IEnumerable BadJsonIdCases => BadJsonIds.Select(data => new TestCaseData(data));

        private static IEnumerable<JValue> JsonIds
        {
            get
            {
                yield return JValue.CreateNull();
                yield return JValue.CreateString("");
                yield return JValue.CreateString("test");
                yield return new JValue(0);
                yield return new JValue(42);
                yield return new JValue(-1);
                yield return new JValue(0L);
                yield return new JValue(42L);
                yield return new JValue(-1L);
                yield return new JValue(int.MaxValue);
                yield return new JValue(long.MaxValue);
            }
        }

        private static IEnumerable<JToken> BadJsonIds
        {
            get
            {
                // Other possible values from ECMA-404, Section 5:
                yield return new JObject();
                yield return new JArray();
                yield return new JValue(0.1);
                yield return new JValue(true);
                yield return new JValue(false);
            }
        }

        private static JsonTextReader CreateJsonReader(string json) => new JsonTextReader(new StringReader(json));
    }
}