using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Models.Response;

namespace Tochka.JsonRpc.Server.Tests.Models.Response
{
    public class JsonResponseWrapperTests
    {
        [Test]
        public void Test_SetId_SetsIfRequest()
        {
            var request = new UntypedRequest()
            {
                RawId = JValue.CreateString("test")
            };
            var value = new JObject();

            new JsonServerResponseWrapper(value, request, null);

            value.Should().ContainKey(JsonRpcConstants.IdProperty);
            Assert.AreEqual(request.RawId, value[JsonRpcConstants.IdProperty]);
        }

        [Test]
        public void Test_SetId_IgnoresIfNotification()
        {
            var notification = new UntypedNotification()
            {
            };
            var value = new JObject();

            new JsonServerResponseWrapper(value, notification, null);

            value.Should().NotContainKey(JsonRpcConstants.IdProperty);
        }

        [Test]
        public async Task Test_Write_SetsResponseProperties()
        {
            var resposneMock = new Mock<HttpResponse>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(resposneMock.Object);
            var next = Mock.Of<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, next)
            {
                WriteResponse = false
            };
            var notification = new UntypedNotification();
            var value = new JObject();
            var wrapper = new JsonServerResponseWrapper(value, notification, null);

            await wrapper.Write(handlingContext, new HeaderJsonRpcSerializer());

            resposneMock.VerifySet(x => x.StatusCode = 200);
            resposneMock.VerifySet(x => x.ContentLength = null);
            resposneMock.VerifySet(x => x.ContentType = It.IsAny<string>());
        }

        [Test]
        public async Task Test_Write_CopiesHeaders()
        {
            var responseHeaders = new HeaderDictionary()
            {
                {"a", "b"}
            };
            var resposneMock = new Mock<HttpResponse>();
            resposneMock.SetupGet(x => x.Headers)
                .Returns(responseHeaders);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(resposneMock.Object);
            var next = Mock.Of<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, next)
            {
                WriteResponse = false
            };
            var notification = new UntypedNotification();
            var value = new JObject();
            var headers = new HeaderDictionary
            {
                {"header", "value"}
            };
            var wrapper = new JsonServerResponseWrapper(value, notification, headers);

            await wrapper.Write(handlingContext, new HeaderJsonRpcSerializer());

            resposneMock.VerifyGet(x => x.Headers);
            responseHeaders.Should().HaveCount(2);
            responseHeaders.Should().ContainKey("header");
        }

        [Test]
        public async Task Test_Write_WritesBody()
        {
            var ms = new MemoryStream();
            var resposneMock = new Mock<HttpResponse>();
            resposneMock.SetupGet(x => x.Body)
                .Returns(ms);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(resposneMock.Object);
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken());
            var next = Mock.Of<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, next)
            {
                WriteResponse = true
            };
            var notification = new UntypedNotification();
            var value = new JObject();
            var wrapper = new JsonServerResponseWrapper(value, notification, null);

            await wrapper.Write(handlingContext, new HeaderJsonRpcSerializer());

            ms.Length.Should().BePositive();
        }

        [Test]
        public async Task Test_Write_ChecksCancellationToken()
        {
            var ms = new MemoryStream();
            var resposneMock = new Mock<HttpResponse>();
            resposneMock.SetupGet(x => x.Body)
                .Returns(ms);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Response)
                .Returns(resposneMock.Object);
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken(true));
            var next = Mock.Of<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, next)
            {
                WriteResponse = true
            };
            var notification = new UntypedNotification();
            var value = new JObject();
            var wrapper = new JsonServerResponseWrapper(value, notification, null);
            Func<Task> action = async () => await wrapper.Write(handlingContext, new HeaderJsonRpcSerializer());

            await action.Should().ThrowAsync<TaskCanceledException>();
        }

        [Test]
        public void Test_Constructor_ThrowsOnNullValue()
        {
            var notification = new UntypedNotification();
            Action action = () => new JsonServerResponseWrapper(null, notification, null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}