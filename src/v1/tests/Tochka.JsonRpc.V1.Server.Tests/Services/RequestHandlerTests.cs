using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server.Exceptions;
using Tochka.JsonRpc.V1.Server.Models.Response;
using Tochka.JsonRpc.V1.Server.Services;
using Tochka.JsonRpc.V1.Server.Settings;
using Tochka.JsonRpc.V1.Server.Tests.Helpers;

namespace Tochka.JsonRpc.V1.Server.Tests.Services
{
    public class RequestHandlerTests
    {
        private TestEnvironment testEnvironment;
        private Mock<RequestHandler> requestHandlerMock;
        private Mock<IJsonRpcErrorFactory> errorFactoryMock;
        private Mock<HeaderJsonRpcSerializer> serializerMock;
        private Mock<INestedContextFactory> contextFactoryMock;
        private Mock<IResponseReader> responseReaderMock;
        private Mock<IOptions<JsonRpcOptions>> optionsMock;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services => { });

            contextFactoryMock = new Mock<INestedContextFactory>
            {
                DefaultValue = DefaultValue.Mock
            };
            errorFactoryMock = new Mock<IJsonRpcErrorFactory>();
            errorFactoryMock.Setup(x => x.Exception(It.IsAny<Exception>()))
                .Returns(new Error<object>());
            errorFactoryMock.Setup(x => x.InvalidRequest(It.IsAny<Exception>()))
                .Returns(new Error<object>());
            serializerMock = new Mock<HeaderJsonRpcSerializer>();
            responseReaderMock = new Mock<IResponseReader>();
            optionsMock = new Mock<IOptions<JsonRpcOptions>>();
            optionsMock.SetupGet(x => x.Value)
                .Returns(new JsonRpcOptions());

            var log = testEnvironment.ServiceProvider.GetRequiredService<ILogger<RequestHandler>>();
            requestHandlerMock = new Mock<RequestHandler>(errorFactoryMock.Object, serializerMock.Object, contextFactoryMock.Object, responseReaderMock.Object, optionsMock.Object, log)
            {
                CallBase = true
            };

        }

        [Test]
        public async Task Test_SafeNext_ChecksCancellation()
        {
            var call = Mock.Of<IUntypedCall>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken(true));
            var next = Mock.Of<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, next);
            requestHandlerMock.Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));

            var result = await requestHandlerMock.Object.SafeNext(call, handlingContext, false);

            result.Should().BeOfType<JsonServerResponseWrapper>();
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<OperationCanceledException>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));
        }

        [Test]
        public async Task Test_SafeNext_WrapsPipelineException()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken(false));
            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .Throws<DivideByZeroException>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, nextMock.Object);
            requestHandlerMock.Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));

            var result = await requestHandlerMock.Object.SafeNext(callMock.Object, handlingContext, false);

            result.Should().BeOfType<JsonServerResponseWrapper>();
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<DivideByZeroException>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));
        }
        
        [Test]
        public async Task Test_SafeNext_WrapsErrorResponseException()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken(false));
            var nextMock = new Mock<RequestDelegate>();
            var errorData = new object();
            var exception = new JsonRpcErrorResponseException(new Error<object>
            {
                Code = 42,
                Data = errorData,
                Message = "test"
            });
            nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .Throws(exception);
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, nextMock.Object);
            requestHandlerMock.Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));

            var result = await requestHandlerMock.Object.SafeNext(callMock.Object, handlingContext, false);

            result.Should().BeOfType<JsonServerResponseWrapper>();
            var response = (result as JsonServerResponseWrapper).Value;
            response["error"].Should().NotBeNull();
            response["error"]["code"].Value<int>().Should().Be(42);
            response["error"]["message"].Value<string>().Should().Be("test");
            response["error"]["data"].Should().NotBeNull();
                
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<Exception>()), Times.Never);
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));
        }

        [Test]
        public async Task Test_SafeNext_WrapsReaderException()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken(false));
            var nextMock = new Mock<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, nextMock.Object);
            responseReaderMock.Setup(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DivideByZeroException());
            requestHandlerMock.Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));

            var result = await requestHandlerMock.Object.SafeNext(callMock.Object, handlingContext, false);

            result.Should().BeOfType<JsonServerResponseWrapper>();
            responseReaderMock.Verify(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<DivideByZeroException>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));
        }

        [Test]
        public async Task Test_SafeNext_ReadsResponse()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken(false));
            var nextMock = new Mock<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, nextMock.Object);
            responseReaderMock.Setup(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IServerResponseWrapper>());
            requestHandlerMock.Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));

            var result = await requestHandlerMock.Object.SafeNext(callMock.Object, handlingContext, false);

            nextMock.Verify(x => x(It.IsAny<HttpContext>()));
            result.Should().BeAssignableTo<IServerResponseWrapper>();
            responseReaderMock.Verify(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));
        }

        [Test]
        public async Task Test_SafeNext_WrapsNullResponse()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestAborted)
                .Returns(new CancellationToken(false));
            var nextMock = new Mock<RequestDelegate>();
            var handlingContext = new HandlingContext(httpContextMock.Object, Encoding.UTF8, nextMock.Object);
            requestHandlerMock.Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));

            var result = await requestHandlerMock.Object.SafeNext(callMock.Object, handlingContext, false);

            nextMock.Verify(x => x(It.IsAny<HttpContext>()));
            result.Should().BeOfType<JsonServerResponseWrapper>();
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<JsonRpcInternalException>()));
            responseReaderMock.Verify(x => x.GetResponse(It.IsAny<HttpContext>(), It.IsAny<IUntypedCall>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<object>()));
        }

        [Test]
        public async Task Test_GetResponseSafeInBatch_ReturnsValue()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            var jToken = JValue.CreateString("test");
            requestHandlerMock.Setup(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false))
                .ReturnsAsync(new JsonServerResponseWrapper(jToken, callMock.Object, Mock.Of<IHeaderDictionary>()));

            var result = await requestHandlerMock.Object.GetResponseSafeInBatch(callMock.Object, handlingContext);

            Assert.AreEqual(jToken, result);
            requestHandlerMock.Verify(x => x.GetResponseSafeInBatch(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false));
        }

        [Test]
        public async Task Test_GetResponseSafeInBatch_WrapsRaw()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false))
                .ReturnsAsync(new RawServerResponseWrapper(Mock.Of<HttpResponse>()));

            await requestHandlerMock.Object.GetResponseSafeInBatch(callMock.Object, handlingContext);

            errorFactoryMock.Verify(x => x.Exception(It.IsAny<JsonRpcInternalException>()));
            requestHandlerMock.Verify(x => x.GetResponseSafeInBatch(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false));
        }

        [Test]
        public async Task Test_GetResponseSafeInBatch_WrapsNull()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false))
                .ReturnsAsync((IServerResponseWrapper) null);

            await requestHandlerMock.Object.GetResponseSafeInBatch(callMock.Object, handlingContext);

            errorFactoryMock.Verify(x => x.Exception(It.IsAny<ArgumentNullException>()));
            requestHandlerMock.Verify(x => x.GetResponseSafeInBatch(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false));
        }

        [Test]
        public async Task Test_GetResponseSafeInBatch_WrapsUnknown()
        {
            var callMock = new Mock<IUntypedCall>();
            callMock.SetupGet(x => x.Jsonrpc)
                .Returns(JsonRpcConstants.Version);
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false))
                .ReturnsAsync(Mock.Of<IServerResponseWrapper>());

            await requestHandlerMock.Object.GetResponseSafeInBatch(callMock.Object, handlingContext);

            errorFactoryMock.Verify(x => x.Exception(It.IsAny<ArgumentOutOfRangeException>()));
            requestHandlerMock.Verify(x => x.GetResponseSafeInBatch(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), false));
        }

        [Test]
        public async Task Test_HandleBatchSequential_ReturnsEmptyOnEmptyBatch()
        {
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());

            var result = await requestHandlerMock.Object.HandleBatchSequential(batch, handlingContext);

            result.Should().BeEmpty();
            requestHandlerMock.Verify(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleBatchSequential_ReturnsEmptyOnNotification()
        {
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
                {
                    new UntypedNotification()
                }
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            var jValue = JValue.CreateString("test");
            requestHandlerMock.Setup(x => x.GetResponseSafeInBatch(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(jValue);

            var result = await requestHandlerMock.Object.HandleBatchSequential(batch, handlingContext);

            result.Should().BeEmpty();
            requestHandlerMock.Verify(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.GetResponseSafeInBatch(It.IsAny<UntypedNotification>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleBatchSequential_ReturnsValueOnRequest()
        {
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
                {
                    new UntypedRequest()
                }
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            var jValue = JValue.CreateString("test");
            requestHandlerMock.Setup(x => x.GetResponseSafeInBatch(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(jValue);

            var result = await requestHandlerMock.Object.HandleBatchSequential(batch, handlingContext);

            result.Should().HaveCount(1);
            Assert.AreEqual(jValue, result[0]);
            requestHandlerMock.Verify(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.GetResponseSafeInBatch(It.IsAny<UntypedRequest>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleBatch_ThrowsOnEmpty()
        {
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            Func<Task> action = async () => await requestHandlerMock.Object.HandleBatch(batch, BatchHandling.Sequential, handlingContext);

            await action.Should().ThrowAsync<JsonRpcInternalException>();

            requestHandlerMock.Verify(x => x.HandleBatch(It.IsAny<BatchRequestWrapper>(), BatchHandling.Sequential, It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleBatch_NoFlagWhenNoRequests()
        {
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
                {
                    new UntypedNotification()
                }
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(new JArray());

            await requestHandlerMock.Object.HandleBatch(batch, BatchHandling.Sequential, handlingContext);

            handlingContext.WriteResponse.Should().BeFalse();
            requestHandlerMock.Verify(x => x.HandleBatch(It.IsAny<BatchRequestWrapper>(), BatchHandling.Sequential, It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleBatch_WritesFlagWhenHaveRequests()
        {
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
                {
                    new UntypedRequest()
                }
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(new JArray());

            await requestHandlerMock.Object.HandleBatch(batch, BatchHandling.Sequential, handlingContext);

            handlingContext.WriteResponse.Should().BeTrue();
            requestHandlerMock.Verify(x => x.HandleBatch(It.IsAny<BatchRequestWrapper>(), BatchHandling.Sequential, It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleBatch_CallsSequential()
        {
            var request = new UntypedRequest();
            var value = new JArray();
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
                {
                    request
                }
            };

            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(value);

            var result = await requestHandlerMock.Object.HandleBatch(batch, BatchHandling.Sequential, handlingContext);

            result.Should().BeOfType<JsonServerResponseWrapper>();
            Assert.AreEqual(value, (result as JsonServerResponseWrapper).Value);
            requestHandlerMock.Verify(x => x.HandleBatch(It.IsAny<BatchRequestWrapper>(), BatchHandling.Sequential, It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleBatchSequential(It.IsAny<BatchRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleBatch_ThrowsOnUnknown()
        {
            var request = new UntypedRequest();
            var batch = new BatchRequestWrapper
            {
                Batch = new List<IUntypedCall>()
                {
                    request
                }
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            Func<Task> action = async () => await requestHandlerMock.Object.HandleBatch(batch, (BatchHandling) (-1), handlingContext);

            await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
            requestHandlerMock.Verify(x => x.HandleBatch(It.IsAny<BatchRequestWrapper>(), It.IsAny<BatchHandling>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleException_WritesFlagAndResponse()
        {
            var responseMock = new Mock<IServerResponseWrapper>();
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            var exception = new DivideByZeroException();
            requestHandlerMock.Setup(x => x.GetResponseWrapper(It.IsAny<JToken>()))
                .Returns(responseMock.Object);

            await requestHandlerMock.Object.HandleException(handlingContext, exception);

            handlingContext.WriteResponse.Should().BeTrue();
            responseMock.Verify(x => x.Write(It.IsAny<HandlingContext>(), It.IsAny<HeaderJsonRpcSerializer>()));
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<DivideByZeroException>()));
            requestHandlerMock.Verify(x => x.HandleException(It.IsAny<HandlingContext>(), It.IsAny<Exception>()));
        }

        [Test]
        public void Test_GetResponseWrapper_ReturnsValue()
        {
            var value = JValue.CreateString("test");
            var result = requestHandlerMock.Object.GetResponseWrapper(value);

            result.Should().NotBeNull();
            requestHandlerMock.Verify(x => x.GetResponseWrapper(It.IsAny<JToken>()));
        }

        [Test]
        public async Task Test_HandleSingle_WritesFlagWhenRequestAndCallsNext()
        {
            var single = new SingleRequestWrapper
            {
                Call = new UntypedRequest()
                {
                    Jsonrpc = JsonRpcConstants.Version
                }
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());

            requestHandlerMock.Setup(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()))
                .ReturnsAsync(Mock.Of<IServerResponseWrapper>());


            await requestHandlerMock.Object.HandleSingle(single, handlingContext);

            handlingContext.WriteResponse.Should().BeTrue();
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.HandleSingle(It.IsAny<SingleRequestWrapper>(), It.IsAny<HandlingContext>()));

            requestHandlerMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Test_HandleSingle_NoFlagWhenNotificationAndCallsNext()
        {
            var single = new SingleRequestWrapper
            {
                Call = new UntypedNotification()
                {
                    Jsonrpc = JsonRpcConstants.Version
                }
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()))
                .ReturnsAsync(Mock.Of<IServerResponseWrapper>());

            await requestHandlerMock.Object.HandleSingle(single, handlingContext);

            handlingContext.WriteResponse.Should().BeFalse();
            requestHandlerMock.Verify(x => x.SafeNext(It.IsAny<IUntypedCall>(), It.IsAny<HandlingContext>(), It.IsAny<bool>()));
            requestHandlerMock.Verify(x => x.HandleSingle(It.IsAny<SingleRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public void Test_HandleBad_WritesFlagAndWrapsError()
        {
            var bad = new BadRequestWrapper
            {
                Exception = new DivideByZeroException()
            };
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.GetError(It.IsAny<DivideByZeroException>()))
                .Returns(new Error<object>());

            var result = requestHandlerMock.Object.HandleBad(bad, handlingContext);

            handlingContext.WriteResponse.Should().BeTrue();
            result.Should().NotBeNull();
            requestHandlerMock.Verify(x => x.GetError(It.IsAny<DivideByZeroException>()));
            requestHandlerMock.Verify(x => x.HandleBad(It.IsAny<BadRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleRequestWrapper_HandlesBad()
        {
            var request = Mock.Of<BadRequestWrapper>();
            var expected = new JsonServerResponseWrapper(JValue.CreateString("test"), Mock.Of<IUntypedCall>(), Mock.Of<IHeaderDictionary>());
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.HandleBad(It.IsAny<BadRequestWrapper>(), It.IsAny<HandlingContext>()))
                .Returns(expected);

            var result = await requestHandlerMock.Object.HandleRequestWrapper(request, handlingContext);

            result.Should().Be(expected);
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleBad(It.IsAny<BadRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleRequestWrapper_HandlesBatch()
        {
            var request = Mock.Of<BatchRequestWrapper>();
            var expected = Mock.Of<IServerResponseWrapper>();
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.HandleBatch(It.IsAny<BatchRequestWrapper>(), It.IsAny<BatchHandling>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(expected);

            var result = await requestHandlerMock.Object.HandleRequestWrapper(request, handlingContext);

            result.Should().Be(expected);
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleBatch(It.IsAny<BatchRequestWrapper>(), It.IsAny<BatchHandling>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleRequestWrapper_HandlesSingle()
        {
            var request = Mock.Of<SingleRequestWrapper>();
            var expected = Mock.Of<IServerResponseWrapper>();
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.HandleSingle(It.IsAny<SingleRequestWrapper>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(expected);

            var result = await requestHandlerMock.Object.HandleRequestWrapper(request, handlingContext);

            result.Should().Be(expected);
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleSingle(It.IsAny<SingleRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleRequestWrapper_HandlesException()
        {
            var request = Mock.Of<IRequestWrapper>();
            var responseMock = new Mock<IServerResponseWrapper>();
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.GetResponseWrapper(It.IsAny<JToken>()))
                .Returns(responseMock.Object);

            var result = await requestHandlerMock.Object.HandleRequestWrapper(request, handlingContext);

            result.Should().Be(responseMock.Object);
            handlingContext.WriteResponse.Should().BeTrue();
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<ArgumentOutOfRangeException>()));
            requestHandlerMock.Verify(x => x.GetResponseWrapper(It.IsAny<JToken>()));
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleRequestWrapper_HandlesNull()
        {
            var responseMock = new Mock<IServerResponseWrapper>();
            var handlingContext = new HandlingContext(Mock.Of<HttpContext>(), Mock.Of<Encoding>(), Mock.Of<RequestDelegate>());
            requestHandlerMock.Setup(x => x.GetResponseWrapper(It.IsAny<JToken>()))
                .Returns(responseMock.Object);

            var result = await requestHandlerMock.Object.HandleRequestWrapper(null, handlingContext);

            result.Should().Be(responseMock.Object);
            handlingContext.WriteResponse.Should().BeTrue();
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<ArgumentOutOfRangeException>()));
            requestHandlerMock.Verify(x => x.GetResponseWrapper(It.IsAny<JToken>()));
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
        }

        [Test]
        public async Task Test_HandleRequest_WritesResponse()
        {
            var request = Mock.Of<IRequestWrapper>();
            var responseMock = new Mock<IServerResponseWrapper>();
            var httpContext = Mock.Of<HttpContext>();
            var encoding = Mock.Of<Encoding>();
            var next = Mock.Of<RequestDelegate>();
            requestHandlerMock.Setup(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()))
                .ReturnsAsync(responseMock.Object);

            await requestHandlerMock.Object.HandleRequest(httpContext, request, encoding, next);

            responseMock.Verify(x => x.Write(It.IsAny<HandlingContext>(), It.IsAny<HeaderJsonRpcSerializer>()));
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleRequest(It.IsAny<HttpContext>(), It.IsAny<IRequestWrapper>(), It.IsAny<Encoding>(), It.IsAny<RequestDelegate>()));
        }

        [Test]
        public async Task Test_HandleRequest_HandlesNull()
        {
            var request = Mock.Of<IRequestWrapper>();
            var httpContext = Mock.Of<HttpContext>();
            var encoding = Mock.Of<Encoding>();
            var next = Mock.Of<RequestDelegate>();
            var exception = Mock.Of<DivideByZeroException>();
            requestHandlerMock.Setup(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()))
                .ThrowsAsync(exception);
            requestHandlerMock.Setup(x => x.HandleException(It.IsAny<HandlingContext>(), It.IsAny<Exception>()))
                .Returns(Task.CompletedTask);

            await requestHandlerMock.Object.HandleRequest(httpContext, request, encoding, next);

            requestHandlerMock.Verify(x => x.HandleException(It.IsAny<HandlingContext>(), It.IsAny<Exception>()));
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleRequest(It.IsAny<HttpContext>(), It.IsAny<IRequestWrapper>(), It.IsAny<Encoding>(), It.IsAny<RequestDelegate>()));
        }

        [Test]
        public async Task Test_HandleRequest_HandlesException()
        {
            var httpContext = Mock.Of<HttpContext>();
            var encoding = Mock.Of<Encoding>();
            var next = Mock.Of<RequestDelegate>();
            var exception = Mock.Of<DivideByZeroException>();
            requestHandlerMock.Setup(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()))
                .ThrowsAsync(exception);
            requestHandlerMock.Setup(x => x.HandleException(It.IsAny<HandlingContext>(), It.IsAny<Exception>()))
                .Returns(Task.CompletedTask);

            await requestHandlerMock.Object.HandleRequest(httpContext, null, encoding, next);

            requestHandlerMock.Verify(x => x.HandleException(It.IsAny<HandlingContext>(), It.IsAny<Exception>()));
            requestHandlerMock.Verify(x => x.HandleRequestWrapper(It.IsAny<IRequestWrapper>(), It.IsAny<HandlingContext>()));
            requestHandlerMock.Verify(x => x.HandleRequest(It.IsAny<HttpContext>(), It.IsAny<IRequestWrapper>(), It.IsAny<Encoding>(), It.IsAny<RequestDelegate>()));
        }

        [TestCase(typeof(JsonRpcInternalException))]
        [TestCase(typeof(ArgumentOutOfRangeException))]
        public void Test_GetError_ReturnsInvalidRequest(Type exceptionType)
        {
            var exception = Activator.CreateInstance(exceptionType, "") as Exception;
            var expected = new Error<object>();
            errorFactoryMock.Setup(x => x.InvalidRequest(It.IsAny<Exception>()))
                .Returns(expected);

            var result = requestHandlerMock.Object.GetError(exception);

            result.Should().Be(expected);
            requestHandlerMock.Verify(x => x.GetError(It.IsAny<Exception>()));
            errorFactoryMock.Verify(x => x.InvalidRequest(It.IsAny<Exception>()));
        }

        [Test]
        public void Test_GetError_ReturnsParseError()
        {
            var exception = new JsonException();
            var expected = new Error<object>();
            errorFactoryMock.Setup(x => x.ParseError(It.IsAny<Exception>()))
                .Returns(expected);

            var result = requestHandlerMock.Object.GetError(exception);

            result.Should().Be(expected);
            requestHandlerMock.Verify(x => x.GetError(It.IsAny<Exception>()));
            errorFactoryMock.Verify(x => x.ParseError(It.IsAny<Exception>()));
        }

        [Test]
        public void Test_GetError_ReturnsExceptionError()
        {
            var exception = new Exception();
            var expected = new Error<object>();
            errorFactoryMock.Setup(x => x.Exception(It.IsAny<Exception>()))
                .Returns(expected);

            var result = requestHandlerMock.Object.GetError(exception);

            result.Should().Be(expected);
            requestHandlerMock.Verify(x => x.GetError(It.IsAny<Exception>()));
            errorFactoryMock.Verify(x => x.Exception(It.IsAny<Exception>()));
        }

        [Test]
        public void Test_PropagateItemsInternal_IgnoresIfNoKey()
        {
            var key = "test";
            var context = MockContext();
            var nestedContext = MockContext();
            context.Object.Items.Should().BeEmpty();
            nestedContext.Object.Items.Should().BeEmpty();
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Verifiable();

            var result = requestHandlerMock.Object.PropagateItemsInternal(context.Object, nestedContext.Object, key);

            result.Should().BeFalse();
            context.Object.Items.Should().BeEmpty();
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(context.Object, nestedContext.Object, key), Times.Once);
        }

        [Test]
        public void Test_PropagateItemsInternal_IgnoresIfNull()
        {
            var key = "test";
            var context = MockContext();
            context.Object.Items.Should().BeEmpty();
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Verifiable();

            var result = requestHandlerMock.Object.PropagateItemsInternal(context.Object, null, key);

            result.Should().BeFalse();
            context.Object.Items.Should().BeEmpty();
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(context.Object, null, key), Times.Once);
        }

        [TestCase("value", "value")]
        [TestCase("", "value")]
        [TestCase(null, "value")]
        [TestCase("value", "")]
        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("value", null)]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void Test_PropagateItemsInternal_SetsNullIfExists(string contextValue, string nestedContextValue)
        {
            var key = "test";
            var context = MockContext();
            var nestedContext = MockContext();
            context.Object.Items[key] = contextValue;
            nestedContext.Object.Items[key] = nestedContextValue;
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Verifiable();

            var result = requestHandlerMock.Object.PropagateItemsInternal(context.Object, nestedContext.Object, key);

            result.Should().BeFalse();
            context.Object.Items.Should().Contain(new KeyValuePair<object, object>(key, null));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(context.Object, nestedContext.Object, key), Times.Once);
        }

        [TestCase("value")]
        [TestCase("")]
        [TestCase(null)]
        public void Test_PropagateItemsInternal_SetsValueIfNotExists(string nestedContextValue)
        {
            var key = "test";
            var context = MockContext();
            var nestedContext = MockContext();
            nestedContext.Object.Items[key] = nestedContextValue;
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Verifiable();

            var result = requestHandlerMock.Object.PropagateItemsInternal(context.Object, nestedContext.Object, key);

            result.Should().BeTrue();
            context.Object.Items.Should().Contain(new KeyValuePair<object, object>(key, nestedContextValue));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(context.Object, nestedContext.Object, key), Times.Once);
        }

        [TestCase("value", "value")]
        [TestCase("", "value")]
        [TestCase(null, "value")]
        [TestCase("value", "")]
        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("value", null)]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void Test_PropagateItemsInternal_SetsNullIfCalledMultipleTimes(string value1, string value2)
        {
            var key = "test";
            var context = MockContext();
            var nestedContext1 = MockContext();
            var nestedContext2 = MockContext();
            context.Object.Items.Should().BeEmpty();
            nestedContext1.Object.Items[key] = value1;
            nestedContext2.Object.Items[key] = value2;
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(It.IsAny<HttpContext>(), It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Verifiable();

            var result1 = requestHandlerMock.Object.PropagateItemsInternal(context.Object, nestedContext1.Object, key);
            var result2 = requestHandlerMock.Object.PropagateItemsInternal(context.Object, nestedContext2.Object, key);

            result1.Should().BeTrue();
            result2.Should().BeFalse();
            context.Object.Items.Should().Contain(new KeyValuePair<object, object>(key, null));
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(context.Object, nestedContext1.Object, key), Times.Once);
            requestHandlerMock.Verify(x => x.PropagateItemsInternal(context.Object, nestedContext2.Object, key), Times.Once);
        }

        [Test]
        public void Test_PropagateItems_CallsInternalWithKeys()
        {
            var context = MockContext();
            var nestedContext = MockContext();
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(context.Object, nestedContext.Object, JsonRpcConstants.ActionDescriptorItemKey))
                .Verifiable();
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(context.Object, nestedContext.Object, JsonRpcConstants.ActionResultTypeItemKey))
                .Verifiable();
            requestHandlerMock
                .Setup(x => x.PropagateItemsInternal(context.Object, nestedContext.Object, JsonRpcConstants.ResponseErrorCodeItemKey))
                .Verifiable();

            requestHandlerMock.Object.PropagateItems(context.Object, nestedContext.Object);

            requestHandlerMock.Verify();
        }

        [TearDown]
        public void TearDown_VerifyAfterTest()
        {
            errorFactoryMock.VerifyNoOtherCalls();
            serializerMock.VerifyNoOtherCalls();
            responseReaderMock.VerifyNoOtherCalls();
            requestHandlerMock.VerifyNoOtherCalls();

            // dont care for nested context factory and options
        }

        private Mock<HttpContext> MockContext()
        {
            var items = new Dictionary<object, object>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items).Returns(items);
            return httpContextMock;
        }
    }
}
