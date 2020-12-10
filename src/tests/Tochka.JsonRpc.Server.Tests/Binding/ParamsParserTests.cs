using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Models.Binding;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Binding
{
    public class ParamsParserTests
    {
        private TestEnvironment testEnvironment;
        private Mock<ParamsParser> paramsParserMock;
        
        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
            });

            paramsParserMock = new Mock<ParamsParser>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParamsParser>>())
            {
                CallBase = true
            };
        }

        [TestCase(BindingStyle.Default, typeof(ErrorParseResult))]
        [TestCase(BindingStyle.Object, typeof(NullParseResult))]
        [TestCase(BindingStyle.Array, typeof(NullParseResult))]
        [TestCase(-1, typeof(ErrorParseResult))]
        public void Test_ParseNull_ChecksBindingStyle(BindingStyle bindingStyle, Type expectedType)
        {
            var result = paramsParserMock.Object.ParseNull(bindingStyle);

            result.Should().BeOfType(expectedType);
            paramsParserMock.Verify(x => x.ParseNull(bindingStyle));
            paramsParserMock.VerifyNoOtherCalls();
        }

        [TestCaseSource(typeof(ParamsParserTests), nameof(ArrayCases))]
        public void Test_ParseArray_ReturnsResult(JArray jArray, int index, BindingStyle style, IParseResult expected)
        {
            var result = paramsParserMock.Object.ParseArray(jArray, index, style);

            result.Should().BeOfType(expected.GetType());
            result.Should().BeEquivalentTo(expected, options =>
                options.Excluding(x => x.RuntimeType == typeof(ErrorParseResult) && x.SelectedMemberInfo.Name == nameof(ErrorParseResult.Message))
            );
            paramsParserMock.Verify(x => x.ParseArray(jArray, index, style));
            paramsParserMock.VerifyNoOtherCalls();
        }

        [TestCaseSource(typeof(ParamsParserTests), nameof(ObjectCases))]
        public void Test_ParseObject_ReturnsResult(JObject jObject, string property, BindingStyle style, IParseResult expected)
        {
            var result = paramsParserMock.Object.ParseObject(jObject, property, style);

            result.Should().BeOfType(expected.GetType());
            result.Should().BeEquivalentTo(expected, options =>
                options.Excluding(x => x.RuntimeType == typeof(ErrorParseResult) && x.SelectedMemberInfo.Name == nameof(ErrorParseResult.Message))
            );
            paramsParserMock.Verify(x => x.ParseObject(jObject, property, style));
            paramsParserMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ParseParams_CallsParseObject()
        {
            var expected = Mock.Of<IParseResult>();
            var parameterMetadata = new ParameterMetadata(new JsonName("test", "test"), typeof(object), 0, BindingStyle.Default, false);
            paramsParserMock.Setup(x => x.ParseObject(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<BindingStyle>()))
                .Returns(expected);

            var result = paramsParserMock.Object.ParseParams(new JObject(), parameterMetadata);

            result.Should().Be(expected);
            paramsParserMock.Verify(x => x.ParseObject(It.IsAny<JObject>(), It.IsAny<string>(), It.IsAny<BindingStyle>()));
            paramsParserMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ParseParams_CallsParseArray()
        {
            var expected = Mock.Of<IParseResult>();
            var parameterMetadata = new ParameterMetadata(new JsonName("test", "test"), typeof(object), 0, BindingStyle.Default, false);
            paramsParserMock.Setup(x => x.ParseArray(It.IsAny<JArray>(), It.IsAny<int>(), It.IsAny<BindingStyle>()))
                .Returns(expected);

            var result = paramsParserMock.Object.ParseParams(new JArray(), parameterMetadata);

            result.Should().Be(expected);
            paramsParserMock.Verify(x => x.ParseArray(It.IsAny<JArray>(), It.IsAny<int>(), It.IsAny<BindingStyle>()));
            paramsParserMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ParseParams_CallsParseNull()
        {
            var expected = Mock.Of<IParseResult>();
            var parameterMetadata = new ParameterMetadata(new JsonName("test", "test"), typeof(object), 0, BindingStyle.Default, false);
            paramsParserMock.Setup(x => x.ParseNull(It.IsAny<BindingStyle>()))
                .Returns(expected);

            var result = paramsParserMock.Object.ParseParams(null, parameterMetadata);

            result.Should().Be(expected);
            paramsParserMock.Verify(x => x.ParseNull(It.IsAny<BindingStyle>()));
            paramsParserMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ParseParams_ReturnsError()
        {
            var parameterMetadata = new ParameterMetadata(new JsonName("test", "test"), typeof(object), 0, BindingStyle.Default, false);

            var result = paramsParserMock.Object.ParseParams(JValue.CreateString(string.Empty), parameterMetadata);

            result.Should().BeOfType<ErrorParseResult>();
            paramsParserMock.VerifyNoOtherCalls();
        }

        private static IEnumerable ArrayCases => ForParseArray.Select(x => new TestCaseData(x.jArray, x.index, x.style, x.expected));

        private static IEnumerable ObjectCases => ForParseObject.Select(x => new TestCaseData(x.jObject, x.property, x.style, x.expected));

        private static IEnumerable<(JArray jArray, int index, BindingStyle style, IParseResult expected)> ForParseArray
        {
            get
            {
                yield return (new JArray {null}, 0, BindingStyle.Default, new NullParseResult("0"));
                yield return (new JArray {JValue.CreateNull()}, 0, BindingStyle.Default, new NullParseResult("0"));
                yield return (new JArray {"test"}, 0, BindingStyle.Default, new SuccessParseResult("test", "0"));
                yield return (new JArray {JValue.CreateString("test")}, 0, BindingStyle.Default, new SuccessParseResult("test", "0"));
                yield return (new JArray { "test1", "test2" }, 1, BindingStyle.Default, new SuccessParseResult("test2", "1"));
                yield return (new JArray(), 0, BindingStyle.Default, new NoParseResult("0"));

                yield return (new JArray(), 0, BindingStyle.Object, new ErrorParseResult(string.Empty, "0"));

                var array = new JArray(); 
                yield return (array, 0, BindingStyle.Array, new SuccessParseResult(array, "0"));

                yield return (new JArray(), 0, (BindingStyle)(-1), new ErrorParseResult(string.Empty, "0"));
            }
        }

        private static IEnumerable<(JObject jObject, string property, BindingStyle style, IParseResult expected)> ForParseObject
        {
            get
            {
                var key = "x";
                yield return (new JObject { [key] = null }, key, BindingStyle.Default, new NullParseResult(key));
                yield return (new JObject { [key] = JValue.CreateNull() }, key, BindingStyle.Default, new NullParseResult(key));
                yield return (new JObject { [key] = "test" }, key, BindingStyle.Default, new SuccessParseResult("test", key));
                yield return (new JObject { [key] = JValue.CreateString("test") }, key, BindingStyle.Default, new SuccessParseResult("test", key));
                yield return (new JObject { [key] = "test1", ["y"] = "test2" }, "y", BindingStyle.Default, new SuccessParseResult("test2", "y"));
                yield return (new JObject(), key, BindingStyle.Default, new NoParseResult(key));

                var jObject = new JObject();
                yield return (jObject, key, BindingStyle.Object, new SuccessParseResult(jObject, key));

                yield return (new JObject(), key, BindingStyle.Array, new ErrorParseResult(string.Empty, key));
                

                yield return (new JObject(), key, (BindingStyle)(-1), new ErrorParseResult(string.Empty, key));
            }
        }
    }
}