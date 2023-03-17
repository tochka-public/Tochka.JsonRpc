using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server.Exceptions;
using Tochka.JsonRpc.V1.Server.Services;
using Tochka.JsonRpc.V1.Server.Tests.Helpers;

namespace Tochka.JsonRpc.V1.Server.Tests.Services
{
    public class RequestReaderTests
    {
        private TestEnvironment testEnvironment;
        private Mock<RequestReader> requestReaderMock;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
            });

            var serializer = new HeaderJsonRpcSerializer();
            var log = testEnvironment.ServiceProvider.GetRequiredService<ILogger<RequestReader>>();
            requestReaderMock = new Mock<RequestReader>(serializer, log)
            {
                CallBase = true
            };
        }

        [Test]
        public async Task Test_ParseRequest_ReadsJsonBodyAndRewinds()
        {
            var json = "[]";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(x => x.Body)
                .Returns(ms);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Request)
                .Returns(requestMock.Object);

            var result = await requestReaderMock.Object.ParseRequest(httpContextMock.Object, Encoding.UTF8);

            result.Should().BeAssignableTo<IRequestWrapper>();
            ms.Position.Should().Be(0);
            requestReaderMock.Verify(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
        }

        [Test]
        public async Task Test_GetRequestWrapper_GetsCachedValue()
        {
            var call = Mock.Of<IUntypedCall>();
            var itemsMock = new Mock<IDictionary<object, object>>();
            itemsMock.Setup(x => x.ContainsKey(JsonRpcConstants.RequestItemKey))
                .Returns(true);
            itemsMock.SetupGet(x => x[JsonRpcConstants.RequestItemKey])
                .Returns(call);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(itemsMock.Object);

            var result = await requestReaderMock.Object.GetRequestWrapper(httpContextMock.Object, Encoding.UTF8);

            result.Should().BeOfType<SingleRequestWrapper>();
            var request = result as SingleRequestWrapper;
            request.Call.Should().Be(call);
            requestReaderMock.Verify(x => x.GetRequestWrapper(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
        }

        [Test]
        public async Task Test_GetRequestWrapper_StoresSingleRequest()
        {
            var notification = new UntypedNotification()
            {
                Method = "test"
            };
            var request = new SingleRequestWrapper()
            {
                Call = notification,
            };
            var itemsMock = new Mock<IDictionary<object, object>>();
            itemsMock.Setup(x => x.ContainsKey(JsonRpcConstants.RequestItemKey))
                .Returns(false);
            itemsMock.SetupSet(x => x[JsonRpcConstants.RequestItemKey] = It.IsAny<IUntypedCall>());
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(itemsMock.Object);
            requestReaderMock.Setup(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()))
                .ReturnsAsync(request);

            var result = await requestReaderMock.Object.GetRequestWrapper(httpContextMock.Object, Encoding.UTF8);

            result.Should().BeOfType<SingleRequestWrapper>();
            result.Should().Be(request);
            var requestResult = result as SingleRequestWrapper;
            requestResult.Call.Should().Be(notification);
            itemsMock.VerifySet(x => x[JsonRpcConstants.RequestItemKey] = It.IsAny<IUntypedCall>());
            requestReaderMock.Verify(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
            requestReaderMock.Verify(x => x.GetRequestWrapper(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
        }

        [Test]
        public async Task Test_GetRequestWrapper_DoesNotStoreBatch()
        {
            var request = new BatchRequestWrapper();
            var itemsMock = new Mock<IDictionary<object, object>>();
            itemsMock.Setup(x => x.ContainsKey(JsonRpcConstants.RequestItemKey))
                .Returns(false);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(itemsMock.Object);
            requestReaderMock.Setup(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()))
                .ReturnsAsync(request);

            var result = await requestReaderMock.Object.GetRequestWrapper(httpContextMock.Object, Encoding.UTF8);

            result.Should().BeOfType<BatchRequestWrapper>();
            result.Should().Be(request);
            requestReaderMock.Verify(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
            requestReaderMock.Verify(x => x.GetRequestWrapper(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
        }

        [Test]
        public async Task Test_GetRequestWrapper_ReturnsBadOnError()
        {
            var itemsMock = new Mock<IDictionary<object, object>>();
            itemsMock.Setup(x => x.ContainsKey(JsonRpcConstants.RequestItemKey))
                .Returns(false);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(itemsMock.Object);
            requestReaderMock.Setup(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()))
                .ThrowsAsync(new DivideByZeroException());

            var result = await requestReaderMock.Object.GetRequestWrapper(httpContextMock.Object, Encoding.UTF8);

            result.Should().BeOfType<BadRequestWrapper>();
            var request = result as BadRequestWrapper;
            request.Exception.Should().BeOfType<DivideByZeroException>();
            requestReaderMock.Verify(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
            requestReaderMock.Verify(x => x.GetRequestWrapper(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
        }

        [Test]
        public async Task Test_GetRequestWrapper_ReturnsBadOnWrongVersion()
        {
            var itemsMock = new Mock<IDictionary<object, object>>();
            itemsMock.Setup(x => x.ContainsKey(JsonRpcConstants.RequestItemKey))
                .Returns(false);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(itemsMock.Object);
            IRequestWrapper wrapper = new SingleRequestWrapper()
            {
                Call = new UntypedRequest()
                {
                    Jsonrpc = "1.0",
                    Method = "test"
                }
            };
            requestReaderMock.Setup(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()))
                .ReturnsAsync(wrapper);

            var result = await requestReaderMock.Object.GetRequestWrapper(httpContextMock.Object, Encoding.UTF8);

            result.Should().BeOfType<BadRequestWrapper>();
            var request = result as BadRequestWrapper;
            request.Exception.Should().BeOfType<JsonRpcInternalException>();
            requestReaderMock.Verify(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
            requestReaderMock.Verify(x => x.GetRequestWrapper(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task Test_GetRequestWrapper_ReturnsBadOnBadMethod(string method)
        {
            var itemsMock = new Mock<IDictionary<object, object>>();
            itemsMock.Setup(x => x.ContainsKey(JsonRpcConstants.RequestItemKey))
                .Returns(false);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(itemsMock.Object);
            IRequestWrapper wrapper = new SingleRequestWrapper()
            {
                Call = new UntypedRequest()
                {
                    Jsonrpc = "2.0",
                    Method = method
                }
            };
            requestReaderMock.Setup(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()))
                .ReturnsAsync(wrapper);

            var result = await requestReaderMock.Object.GetRequestWrapper(httpContextMock.Object, Encoding.UTF8);

            result.Should().BeOfType<BadRequestWrapper>();
            var request = result as BadRequestWrapper;
            request.Exception.Should().BeOfType<JsonRpcInternalException>();
            requestReaderMock.Verify(x => x.ParseRequest(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
            requestReaderMock.Verify(x => x.GetRequestWrapper(It.IsAny<HttpContext>(), It.IsAny<Encoding>()));
        }

        [TearDown]
        public void TearDown_VerifyAfterTest()
        {
            requestReaderMock.VerifyNoOtherCalls();
        }
    }
}
