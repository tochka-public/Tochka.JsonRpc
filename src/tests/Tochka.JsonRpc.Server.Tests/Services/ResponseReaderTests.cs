using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Models.Response;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Services
{
    public class ResponseReaderTests
    {
        private TestEnvironment testEnvironment;
        private Mock<IJsonRpcErrorFactory> errorFactoryMock;
        private HeaderJsonRpcSerializer serializer;
        private Mock<ResponseReader> responseReaderMock;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
            });

            errorFactoryMock = new Mock<IJsonRpcErrorFactory>();
            serializer = new HeaderJsonRpcSerializer();

            var log = testEnvironment.ServiceProvider.GetRequiredService<ILogger<ResponseReader>>();
            responseReaderMock = new Mock<ResponseReader>(errorFactoryMock.Object, serializer, log)
            {
                CallBase = true
            };
        }

        [Test]
        public void Test_FormatHttpErrorResponse_CallsMehtodNotFound()
        {
            var error = new Error<object>
            {
                Code = 0,
            };
            var responseMock = new Mock<HttpResponse>();
            responseMock.Setup(x => x.StatusCode)
                .Returns(404);
            errorFactoryMock.Setup(x => x.MethodNotFound(It.IsAny<object>()))
                .Returns(error);

            var result =  responseReaderMock.Object.FormatHttpErrorResponse(responseMock.Object, string.Empty);

            result.Should().BeAssignableTo<JToken>();
            errorFactoryMock.Verify(x => x.MethodNotFound(It.IsAny<object>()));
            responseReaderMock.Verify(x => x.FormatHttpErrorResponse(It.IsAny<HttpResponse>(), It.IsAny<string>()));
        }

        [TestCase(401)]
        [TestCase(403)]
        [TestCase(500)]
        [TestCase(502)]
        public void Test_FormatHttpErrorResponse_CallsInternalError(int code)
        {
            var error = new Error<object>
            {
                Code = 0,
            };
            var responseMock = new Mock<HttpResponse>();
            responseMock.Setup(x => x.StatusCode)
                .Returns(code);
            errorFactoryMock.Setup(x => x.InternalError(It.IsAny<object>()))
                .Returns(error);

            var result = responseReaderMock.Object.FormatHttpErrorResponse(responseMock.Object, string.Empty);

            result.Should().BeAssignableTo<JToken>();
            errorFactoryMock.Verify(x => x.InternalError(It.IsAny<object>()));
            responseReaderMock.Verify(x => x.FormatHttpErrorResponse(It.IsAny<HttpResponse>(), It.IsAny<string>()));
        }

        [Test]
        public void Test_ReadTextBody_ReturnsNullOnEmptyStream()
        {
            var ms = new MemoryStream();
            var encoding = Encoding.UTF8;

            var result = responseReaderMock.Object.ReadTextBody(ms, encoding);

            result.Should().BeNull();
            responseReaderMock.Verify(x => x.ReadTextBody(It.IsAny<MemoryStream>(), It.IsAny<Encoding>()));
        }

        [Test]
        public void Test_ReadTextBody_ReadsStringWithEncoding()
        {
            var encoding = Encoding.UTF8;
            var value = "test";
            var ms = new MemoryStream(encoding.GetBytes(value));

            var result = responseReaderMock.Object.ReadTextBody(ms, encoding);

            result.Should().Be(value);
            responseReaderMock.Verify(x => x.ReadTextBody(It.IsAny<MemoryStream>(), It.IsAny<Encoding>()));
        }

        [Test]
        public async Task Test_ReadJsonResponse_ReadsJson()
        {
            var encoding = Encoding.UTF8;
            var value = "{}";
            var ms = new MemoryStream(encoding.GetBytes(value));

            var result = await responseReaderMock.Object.ReadJsonResponse(ms, encoding, new CancellationToken());

            result.Should().BeOfType<JObject>();
            responseReaderMock.Verify(x => x.ReadJsonResponse(It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_ReadJsonResponse_ReadsJsonAndDoNotConvertDateTimeFormat()
        {
            var encoding = Encoding.UTF8;
            var value = @"{
  ""created_at"": ""2022-05-05T05:05:05.123000Z""
}";
            var ms = new MemoryStream(encoding.GetBytes(value));

            var result = await responseReaderMock.Object.ReadJsonResponse(ms, encoding, new CancellationToken());

            result.ToString().Should().Be(value);
            responseReaderMock.Verify(x => x.ReadJsonResponse(It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_HandleUnknownResult_ReturnsRaw()
        {
            var responseMock = new Mock<HttpResponse>();

            var result = await responseReaderMock.Object.HandleUnknownResult(responseMock.Object, Mock.Of<IUntypedCall>(), Mock.Of<MemoryStream>(), Mock.Of<Encoding>(), new CancellationToken());

            result.Should().BeOfType<RawServerResponseWrapper>();
            responseReaderMock.Verify(x => x.HandleUnknownResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_HandleNullResult_ReadsAndFormatsAndReturnsJson()
        {
            var json = JValue.CreateString("test");
            var responseMock = new Mock<HttpResponse>();
            var headers = Mock.Of<IHeaderDictionary>();
            responseMock.SetupGet(x => x.Headers)
                .Returns(headers);
            responseReaderMock.Setup(x => x.ReadTextBody(It.IsAny<MemoryStream>(), It.IsAny<Encoding>()))
                .Returns(string.Empty);
            responseReaderMock.Setup(x => x.FormatHttpErrorResponse(It.IsAny<HttpResponse>(), It.IsAny<string>()))
                .Returns(json);

            var result = await responseReaderMock.Object.HandleNullResult(responseMock.Object, Mock.Of<IUntypedCall>(), Mock.Of<MemoryStream>(), Mock.Of<Encoding>(), new CancellationToken());

            result.Should().BeOfType<JsonServerResponseWrapper>();
            var jsonResult = result as JsonServerResponseWrapper;
            Assert.AreEqual(json, jsonResult.Value);
            Assert.AreEqual(headers, jsonResult.Headers);

            responseReaderMock.Verify(x => x.FormatHttpErrorResponse(It.IsAny<HttpResponse>(), It.IsAny<string>()));
            responseReaderMock.Verify(x => x.ReadTextBody(It.IsAny<MemoryStream>(), It.IsAny<Encoding>()));
            responseReaderMock.Verify(x => x.HandleNullResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_HandleObjectResult_ReadsAndReturnsJson()
        {
            var json = JValue.CreateString("test");
            var responseMock = new Mock<HttpResponse>();
            var headers = Mock.Of<IHeaderDictionary>();
            responseMock.SetupGet(x => x.Headers)
                .Returns(headers);
            responseReaderMock.Setup(x => x.ReadJsonResponse(It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(json);

            var result = await responseReaderMock.Object.HandleObjectResult(responseMock.Object, Mock.Of<IUntypedCall>(), Mock.Of<MemoryStream>(), Mock.Of<Encoding>(), new CancellationToken());

            result.Should().BeOfType<JsonServerResponseWrapper>();
            var jsonResult = result as JsonServerResponseWrapper;
            Assert.AreEqual(json, jsonResult.Value);
            Assert.AreEqual(headers, jsonResult.Headers);

            responseReaderMock.Verify(x => x.ReadJsonResponse(It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.HandleObjectResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_HandleResultType_ObjectResultCase()
        {
            var expected = Mock.Of<IServerResponseWrapper>();
            var responseMock = new Mock<HttpResponse>();
            var headers = Mock.Of<IHeaderDictionary>();
            responseMock.SetupGet(x => x.Headers)
                .Returns(headers);
            responseReaderMock.Setup(x => x.HandleObjectResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await responseReaderMock.Object.HandleResultType(responseMock.Object, typeof(ObjectResult), Mock.Of<IUntypedCall>(), Mock.Of<MemoryStream>(), Mock.Of<Encoding>(), new CancellationToken());

            result.Should().Be(expected);

            responseReaderMock.Verify(x => x.HandleObjectResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.HandleResultType(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_HandleResultType_NullResultCase()
        {
            var expected = Mock.Of<IServerResponseWrapper>();
            var responseMock = new Mock<HttpResponse>();
            var headers = Mock.Of<IHeaderDictionary>();
            responseMock.SetupGet(x => x.Headers)
                .Returns(headers);
            responseReaderMock.Setup(x => x.HandleNullResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await responseReaderMock.Object.HandleResultType(responseMock.Object, null, Mock.Of<IUntypedCall>(), Mock.Of<MemoryStream>(), Mock.Of<Encoding>(), new CancellationToken());

            result.Should().Be(expected);

            responseReaderMock.Verify(x => x.HandleNullResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.HandleResultType(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_HandleResultType_UnknownResultCase()
        {
            var expected = Mock.Of<IServerResponseWrapper>();
            var responseMock = new Mock<HttpResponse>();
            var headers = Mock.Of<IHeaderDictionary>();
            responseMock.SetupGet(x => x.Headers)
                .Returns(headers);
            responseReaderMock.Setup(x => x.HandleUnknownResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await responseReaderMock.Object.HandleResultType(responseMock.Object, typeof(int), Mock.Of<IUntypedCall>(), Mock.Of<MemoryStream>(), Mock.Of<Encoding>(), new CancellationToken());

            result.Should().Be(expected);

            responseReaderMock.Verify(x => x.HandleUnknownResult(It.IsAny<HttpResponse>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.HandleResultType(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_GetResponseWrapper_ThrowsOnNotMemoryStream()
        {
            var responseMock = new Mock<HttpResponse>();
            responseMock.SetupGet(x => x.Body)
                .Returns(Mock.Of<Stream>());
            Func<Task> action = async () => await responseReaderMock.Object.GetResponseWrapper(responseMock.Object, typeof(int), Mock.Of<IUntypedCall>(), new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcInternalException>();

            responseReaderMock.Verify(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_GetResponseWrapper_SeeksAndCallsHandleResultType()
        {
            var ms = new MemoryStream(new byte[1]);
            ms.ReadByte();
            var expected = Mock.Of<IServerResponseWrapper>();
            var responseMock = new Mock<HttpResponse>();
            var headers = Mock.Of<IHeaderDictionary>();
            responseMock.SetupGet(x => x.Headers)
                .Returns(headers);
            responseMock.SetupGet(x => x.Body)
                .Returns(ms);
            responseReaderMock.Setup(x => x.HandleResultType(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await responseReaderMock.Object.GetResponseWrapper(responseMock.Object, typeof(int), Mock.Of<IUntypedCall>(), new CancellationToken());

            result.Should().Be(expected);
            ms.Position.Should().Be(0);
            responseReaderMock.Verify(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.HandleResultType(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_GetResponseWrapper_SeeksAndCallsHandleResultTypeGetsEncoding()
        {
            var ms = new MemoryStream(new byte[1]);
            ms.ReadByte();
            var expected = Mock.Of<IServerResponseWrapper>();
            var responseMock = new Mock<HttpResponse>();
            var headers = new HeaderDictionary
            {
                ["Content-Type"] = JsonRpcConstants.ContentType
            };
            responseMock.SetupGet(x => x.Headers)
                .Returns(headers);
            responseMock.SetupGet(x => x.Body)
                .Returns(ms);
            responseReaderMock.Setup(x => x.HandleResultType(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await responseReaderMock.Object.GetResponseWrapper(responseMock.Object, typeof(int), Mock.Of<IUntypedCall>(), new CancellationToken());

            result.Should().Be(expected);
            ms.Position.Should().Be(0);
            responseReaderMock.Verify(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.HandleResultType(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<MemoryStream>(), It.IsAny<Encoding>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_GetResponse_ThrowsOnRawWhenNotAllowed()
        {
            var responseMock = new Mock<HttpResponse>();
            var items = new Dictionary<object, object> {[JsonRpcConstants.ActionResultTypeItemKey] = Mock.Of<Type>()};
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(responseMock.Object);
            httpContextMock.SetupGet(x => x.Items)
                .Returns(items);
            var expected = new RawServerResponseWrapper(responseMock.Object);
            responseReaderMock.Setup(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
            Func<Task> action = async () => await responseReaderMock.Object.GetResponse(httpContextMock.Object, Mock.Of<IUntypedCall>(), false, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcInternalException>();

            responseReaderMock.Verify(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_GetResponse_ReturnsRawWhenAllowed()
        {
            var responseMock = new Mock<HttpResponse>();
            var items = new Dictionary<object, object> { [JsonRpcConstants.ActionResultTypeItemKey] = Mock.Of<Type>() };
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(responseMock.Object);
            httpContextMock.SetupGet(x => x.Items)
                .Returns(items);
            var expected = new RawServerResponseWrapper(responseMock.Object);
            responseReaderMock.Setup(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await responseReaderMock.Object.GetResponse(httpContextMock.Object, Mock.Of<IUntypedCall>(), true, new CancellationToken());

            result.Should().Be(expected);
            responseReaderMock.Verify(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Test_GetResponse_ThrowsOnNull(bool allowRaw)
        {
            var responseMock = new Mock<HttpResponse>();
            var items = new Dictionary<object, object> { [JsonRpcConstants.ActionResultTypeItemKey] = Mock.Of<Type>() };
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(responseMock.Object);
            httpContextMock.SetupGet(x => x.Items)
                .Returns(items);
            responseReaderMock.Setup(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServerResponseWrapper)null);
            Func<Task> action = async () => await responseReaderMock.Object.GetResponse(httpContextMock.Object, Mock.Of<IUntypedCall>(), allowRaw, new CancellationToken());

            await action.Should().ThrowAsync<ArgumentNullException>();

            responseReaderMock.Verify(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Test_GetResponse_ReturnsValue(bool allowRaw)
        {
            var responseMock = new Mock<HttpResponse>();
            var items = new Dictionary<object, object> { [JsonRpcConstants.ActionResultTypeItemKey] = Mock.Of<Type>() };
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(responseMock.Object);
            httpContextMock.SetupGet(x => x.Items)
                .Returns(items);
            responseReaderMock.Setup(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IServerResponseWrapper)null);
            var expected = Mock.Of<IServerResponseWrapper>();
            responseReaderMock.Setup(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await responseReaderMock.Object.GetResponse(httpContextMock.Object, Mock.Of<IUntypedCall>(), allowRaw, new CancellationToken());

            result.Should().Be(expected);
            responseReaderMock.Verify(x => x.GetResponseWrapper(It.IsAny<HttpResponse>(), It.IsAny<Type>(), It.IsAny<IUntypedCall>(), It.IsAny<CancellationToken>()));
            responseReaderMock.Verify(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        }

        [TearDown]
        public void TearDown_VerifyAfterTest()
        {
            errorFactoryMock.VerifyNoOtherCalls();
            responseReaderMock.VerifyNoOtherCalls();
        }
    }
}
