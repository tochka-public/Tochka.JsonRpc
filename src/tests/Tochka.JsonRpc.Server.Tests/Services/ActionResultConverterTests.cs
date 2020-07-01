using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Services
{
    public class ActionResultConverterTests
    {
        private TestEnvironment testEnvironment;
        private JsonRpcOptions jsonRpcOptions;
        private static readonly Mock<JsonRpcFormatter> FormatterMock = new Mock<JsonRpcFormatter>(new HeaderJsonRpcSerializer(), Mock.Of<ArrayPool<char>>());

        [SetUp]
        public void Setup()
        {
            jsonRpcOptions = new JsonRpcOptions();
            var optionsMock = new Mock<IOptions<JsonRpcOptions>>();
            optionsMock.Setup(x => x.Value)
                .Returns(jsonRpcOptions);
            testEnvironment = new TestEnvironment(services =>
            {
                services.AddSingleton(FormatterMock.Object);
                services.AddSingleton<IJsonRpcErrorFactory, JsonRpcErrorFactory>();
                services.AddSingleton(optionsMock.Object);
            });
        }

        [Test]
        public void Test_SerializeContent_ReturnsNull()
        {
            var converter = GetConverterMock();

            var result = converter.Object.SerializeContent(null, Mock.Of<IJsonRpcSerializer>());

            Assert.AreEqual(JValue.CreateNull(), result);
            converter.Verify(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_SerializeContent_ReturnsJson()
        {
            var converter = GetConverterMock();
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());

            var result = converter.Object.SerializeContent("test", serializerMock.Object);

            Assert.AreEqual(JValue.CreateString("test"), result);
            converter.Verify(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ResponseFromObject_ReturnsFormattedErrorResponse()
        {
            var expected = JValue.CreateString("test");
            var converter = GetConverterMock();
            converter.Setup(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()))
                .Returns(expected);

            var result = converter.Object.ResponseFromObject(Mock.Of<IError>(), Mock.Of<IJsonRpcSerializer>());

            result.Should().BeOfType<UntypedErrorResponse>();
            var response = result as UntypedErrorResponse;
            response.Error.GetData().Should().Be(expected);
            converter.Verify(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.Verify(x => x.ResponseFromObject(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.Verify(x => x.GetErrorResponse(It.IsAny<IError>(), It.IsAny<IJsonRpcSerializer>()));
            converter.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ResponseFromObject_ReturnsFormattedResponse()
        {
            var expected = JValue.CreateString("test");
            var converter = GetConverterMock();
            converter.Setup(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()))
                .Returns(expected);

            var result = converter.Object.ResponseFromObject("test", Mock.Of<IJsonRpcSerializer>());

            result.Should().BeOfType<UntypedResponse>();
            Assert.AreEqual(expected, (result as UntypedResponse).Result);
            converter.Verify(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.Verify(x => x.ResponseFromObject(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_MaybeHttpCodeErrorResponse_ReturnsNullOnGoodCode()
        {
            var converter = GetConverterMock();

            var result = converter.Object.MaybeHttpCodeErrorResponse(200, "test", Mock.Of<IJsonRpcSerializer>());

            result.Should().BeNull();
            converter.Verify(x => x.MaybeHttpCodeErrorResponse(It.IsAny<int>(), It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_MaybeHttpCodeErrorResponse_ReturnsFormattedErrorResponseOnBadCode()
        {
            var expected = JValue.CreateString("test");
            var converter = GetConverterMock();
            converter.Setup(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()))
                .Returns(expected);

            var result = converter.Object.MaybeHttpCodeErrorResponse(500, "test", Mock.Of<IJsonRpcSerializer>());

            result.Should().BeOfType<UntypedErrorResponse>();
            var response = result as UntypedErrorResponse;
            response.Error.GetData().Should().Be(expected);
            converter.Verify(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.Verify(x => x.MaybeHttpCodeErrorResponse(It.IsAny<int>(), It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
            converter.VerifyNoOtherCalls();
        }

        [TestCase(200)]
        [TestCase(500)]
        public void Test_CreateObjectResult_ReturnsObjectResult(int httpCode)
        {
            var jValue = JValue.CreateString("test");
            var converter = GetConverterMock();
            converter.Setup(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()))
                .Returns(jValue);

            var result = converter.Object.CreateObjectResult(httpCode, "test", Mock.Of<IJsonRpcSerializer>());

            result.StatusCode.Should().Be(200);
            result.Formatters.Should().HaveCount(1);
            result.Formatters.Should().HaveElementAt(0, testEnvironment.ServiceProvider.GetRequiredService<JsonRpcFormatter>());
            result.ContentTypes.Should().HaveCount(1);
            result.ContentTypes.Should().HaveElementAt(0, JsonRpcConstants.ContentType);
            result.Value.Should().BeAssignableTo<IResponse>();
            converter.Verify(x => x.SerializeContent(It.IsAny<object>(), It.IsAny<IJsonRpcSerializer>()));
        }

        [Test]
        public void Test_GetFailedBindingResult_ReturnsBadRequestObjectResult()
        {
            var converter = GetConverterMock();

            var result = converter.Object.GetFailedBindingResult(Mock.Of<ModelStateDictionary>());

            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = result as BadRequestObjectResult;
            badResult.ContentTypes.Should().HaveCount(1);
            badResult.ContentTypes.Should().HaveElementAt(0, JsonRpcConstants.ContentType);
            converter.Verify(x => x.GetFailedBindingResult(It.IsAny<ModelStateDictionary>()));
            converter.VerifyNoOtherCalls();
        }

        [TestCaseSource(typeof(ActionResultConverterTests), nameof(ActionResultCases))]
        public void Test_ParseObject_ReturnsResult(IActionResult input, IActionResult expected)
        {
            jsonRpcOptions.AllowRawResponses = true;
            var converter = GetConverterMock();
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            var metadata = new MethodMetadata(new JsonRpcMethodOptions(), new JsonName("", ""), new JsonName("", ""));

            var result = converter.Object.ConvertActionResult(input, metadata, serializerMock.Object);

            if (expected is ObjectResult expectedObjectResult)
            {
                var objectResult = result as ObjectResult;
                objectResult.Value.Should().BeOfType(expectedObjectResult.Value.GetType());
                objectResult.ContentTypes.Should().BeEquivalentTo(expectedObjectResult.ContentTypes);
                objectResult.Formatters.Should().BeEquivalentTo(expectedObjectResult.Formatters);
                objectResult.StatusCode.Should().Be(expectedObjectResult.StatusCode);
            }
            else
            {
                result.Should().Be(expected);
            }
        }

        [Test]
        public void Test_ParseObject_ThrowsOnUnknownIfNotAllowed()
        {
            jsonRpcOptions.AllowRawResponses = false;
            var converter = GetConverterMock();
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            var metadata = new MethodMetadata(new JsonRpcMethodOptions(), new JsonName("", ""), new JsonName("", ""));
            Action action = () => converter.Object.ConvertActionResult(new FileContentResult(new byte[] { }, "application/octet-stream"), metadata, serializerMock.Object);

            action.Should().Throw<JsonRpcInternalException>();
        }


        private static IEnumerable ActionResultCases => ForConvertActionResult.Select(x => new TestCaseData(x.input, x.expected));

        private static IEnumerable<(IActionResult input, IActionResult expected)> ForConvertActionResult
        {
            get
            {
                var defaultFormatters = new FormatterCollection<IOutputFormatter>
                {
                    FormatterMock.Object
                };
                var defaultContentTypes = new MediaTypeCollection()
                {
                    JsonRpcConstants.ContentType
                };

                yield return (
                    new ObjectResult("test"),
                    new ObjectResult(new UntypedResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                yield return (
                    new ObjectResult(null),
                    new ObjectResult(new UntypedResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                yield return (
                    new ObjectResult("test")
                    {
                        StatusCode = 200
                    },
                    new ObjectResult(new UntypedResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                yield return (
                    new ObjectResult("test")
                    {
                        StatusCode = 400
                    },
                    new ObjectResult(new UntypedErrorResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                yield return (
                    new ObjectResult(new Error<object>
                    {
                        Code = 0,
                        Message = "test"
                    }),
                    new ObjectResult(new UntypedErrorResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                yield return (
                    new StatusCodeResult(200),
                    new ObjectResult(new UntypedResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                yield return (
                    new StatusCodeResult(400),
                    new ObjectResult(new UntypedErrorResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                yield return (
                    new EmptyResult(),
                    new ObjectResult(new UntypedResponse())
                    {
                        StatusCode = 200,
                        Formatters = defaultFormatters,
                        ContentTypes = defaultContentTypes
                    }
                );

                var fileResult = new FileContentResult(new byte[] { }, "application/octet-stream");
                yield return (
                    fileResult,
                    fileResult
                );
            }
        }

        private Mock<ActionResultConverter> GetConverterMock()
        {
            return new Mock<ActionResultConverter>(testEnvironment.ServiceProvider.GetRequiredService<JsonRpcFormatter>(), testEnvironment.ServiceProvider.GetRequiredService<IJsonRpcErrorFactory>(), testEnvironment.ServiceProvider.GetRequiredService<IOptions<JsonRpcOptions>>(), testEnvironment.ServiceProvider.GetRequiredService<ILogger<ActionResultConverter>>())
            {
                CallBase = true
            };
        }
    }
}