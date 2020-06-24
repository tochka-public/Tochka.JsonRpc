using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Models.Response;

namespace Tochka.JsonRpc.Server.Tests.Models.Response
{
    public class RawResponseWrapperTests
    {
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
            var sourceMock = new Mock<HttpResponse>();
            sourceMock.SetupGet(x => x.StatusCode)
                .Returns(201);
            var wrapper = new RawServerResponseWrapper(sourceMock.Object);

            await wrapper.Write(handlingContext, new HeaderRpcSerializer());

            resposneMock.VerifySet(x => x.StatusCode = 201);
            resposneMock.VerifySet(x => x.ContentLength = null);
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
            var headers = new HeaderDictionary
            {
                {"header", "value"}
            };
            var sourceMock = new Mock<HttpResponse>();
            sourceMock.SetupGet(x => x.StatusCode)
                .Returns(201);
            sourceMock.SetupGet(x => x.Headers)
                .Returns(headers);
            var wrapper = new RawServerResponseWrapper(sourceMock.Object);

            await wrapper.Write(handlingContext, new HeaderRpcSerializer());

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
            var sourceResponse = new MemoryStream(new byte[1]);
            var sourceMock = new Mock<HttpResponse>();
            sourceMock.SetupGet(x => x.StatusCode)
                .Returns(200);
            sourceMock.SetupGet(x => x.Body)
                .Returns(sourceResponse);
            var wrapper = new RawServerResponseWrapper(sourceMock.Object);

            await wrapper.Write(handlingContext, new HeaderRpcSerializer());

            ms.Length.Should().BePositive();
        }

        [Test]
        public async Task Test_Write_ChecksMemoryStream()
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
            var sourceResponse = Mock.Of<Stream>();
            var sourceMock = new Mock<HttpResponse>();
            sourceMock.SetupGet(x => x.StatusCode)
                .Returns(200);
            sourceMock.SetupGet(x => x.Body)
                .Returns(sourceResponse);
            var wrapper = new RawServerResponseWrapper(sourceMock.Object);

            Func<Task> action = async () => await wrapper.Write(handlingContext, new HeaderRpcSerializer());

            await action.Should().ThrowAsync<JsonRpcInternalException>();
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
            var sourceResponse = new MemoryStream(new byte[1]);
            var sourceMock = new Mock<HttpResponse>();
            sourceMock.SetupGet(x => x.StatusCode)
                .Returns(200);
            sourceMock.SetupGet(x => x.Body)
                .Returns(sourceResponse);
            var wrapper = new RawServerResponseWrapper(sourceMock.Object);

            Func<Task> action = async () => await wrapper.Write(handlingContext, new HeaderRpcSerializer());

            await action.Should().ThrowAsync<TaskCanceledException>();
        }
    }
}