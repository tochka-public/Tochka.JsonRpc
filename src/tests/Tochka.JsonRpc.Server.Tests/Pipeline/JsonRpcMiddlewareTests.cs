using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Pipeline
{
    public class JsonRpcMiddlewareTests
    {
        private TestEnvironment testEnvironment;
        private Mock<IRequestReader> readerMock;
        private Mock<RequestDelegate> nextMock;
        private Mock<IJsonRpcRoutes> routesMock;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
                readerMock = new Mock<IRequestReader>();
                nextMock = new Mock<RequestDelegate>();
                routesMock = new Mock<IJsonRpcRoutes>();
                routesMock.Setup(x => x.IsJsonRpcRoute(It.IsAny<string>()))
                    .Returns(true);
                services.AddSingleton(readerMock.Object);
                services.AddSingleton(nextMock.Object);
                services.AddSingleton<JsonRpcMiddleware>();
            });
        }

        [Test]
        public async Task Test_Invoke_CallsNextIfNotJsonRpc()
        {
            var middleware = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcMiddleware>();
            var headersMock = new Mock<IHeaderDictionary>();
            var requestMock = new Mock<HttpRequest>();
            requestMock.SetupGet(x => x.Headers)
                .Returns(headersMock.Object);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Request)
                .Returns(requestMock.Object);
            var handlerMock = new Mock<IRequestHandler>();

            await middleware.Invoke(httpContextMock.Object, handlerMock.Object, routesMock.Object);

            nextMock.Verify(x => x(httpContextMock.Object));
            nextMock.VerifyNoOtherCalls();
            readerMock.VerifyNoOtherCalls();
            handlerMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Test_Invoke_CallsReaderHandlerIfJsonRpc()
        {
            var middleware = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcMiddleware>();
            var headers = new HeaderDictionary
            {
                ["Content-Type"] = JsonRpcConstants.ContentType
            };
            var requestMock = new Mock<HttpRequest>();
            requestMock.SetupGet(x => x.Headers)
                .Returns(headers);
            requestMock.SetupGet(x => x.Method)
                .Returns(HttpMethods.Post);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Request)
                .Returns(requestMock.Object);
            var handlerMock = new Mock<IRequestHandler>();

            await middleware.Invoke(httpContextMock.Object, handlerMock.Object, routesMock.Object);

            nextMock.VerifyNoOtherCalls();
            readerMock.Verify(x => x.GetRequestWrapper(httpContextMock.Object, It.IsAny<Encoding>()));
            readerMock.VerifyNoOtherCalls();
            handlerMock.Verify(x => x.HandleRequest(httpContextMock.Object, It.IsAny<IRequestWrapper>(), It.IsAny<Encoding>(), nextMock.Object));
            handlerMock.VerifyNoOtherCalls();
        }
    }
}