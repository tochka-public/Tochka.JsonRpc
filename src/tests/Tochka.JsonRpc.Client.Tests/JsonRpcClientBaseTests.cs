using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Settings;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tests
{
    public class JsonRpcClientBaseTests
    {
        private Mock<JsonRpcClientBase> clientMock;
        private TestEnvironment testEnvironment;
        private Mock<IJsonRpcSerializer> serializerMock;
        private Mock<IJsonRpcIdGenerator> generatorMock;
        private Mock<JsonRpcClientOptionsBase> optionsMock;
        private MockHttpMessageHandler handlerMock;

        [SetUp]
        public void Setup()
        {
            handlerMock = new MockHttpMessageHandler();
            serializerMock = new Mock<IJsonRpcSerializer>();
            optionsMock = new Mock<JsonRpcClientOptionsBase>()
            {
                CallBase = true
            };
            optionsMock.Object.Url = "http://foo.bar/";
            generatorMock = new Mock<IJsonRpcIdGenerator>();
            testEnvironment = new TestEnvironment();
            clientMock = new Mock<JsonRpcClientBase>(handlerMock.ToHttpClient(), serializerMock.Object, new HeaderJsonRpcSerializer(), optionsMock.Object, generatorMock.Object, testEnvironment.ServiceProvider.GetRequiredService<ILogger<JsonRpcClientBase>>())
            {
                CallBase = true
            };
            //clientMock.Setup(x => x.InitializeClient(It.IsAny<HttpClient>(), It.IsAny<JsonRpcClientOptionsBase>()));
        }

        [Test]
        public void Test_UserAgent_DefaultValue()
        {
            clientMock.Object.UserAgent.Should().Be("Tochka.JsonRpc.Client");
        }

        [Test]
        public void Test_Encoding_DefaultValue()
        {
            clientMock.Object.Encoding.Should().Be(Encoding.UTF8);
        }

        [Test]
        public void Test_Constructor_CallsInitializeClient()
        {
            var client = clientMock.Object;

            clientMock.Verify(x => x.InitializeClient(It.IsAny<HttpClient>(), It.IsAny<JsonRpcClientOptionsBase>()));
        }

        [Test]
        public async Task Test_SendNotification1_ChainsToActualMehtod()
        {
            var notification = new Notification<object>();
            clientMock.Setup(x => x.SendNotification(null, notification, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await clientMock.Object.SendNotification(notification, new CancellationToken());

            clientMock.Verify(x => x.SendNotification(null, notification, It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendNotification2_ChainsToActualMehtod()
        {
            var url = "test";
            var method = "method";
            var parameters = new object();
            
            clientMock.Setup(x => x.SendNotification(url, It.Is<Notification<object>>(y => y.Method == method && y.Params == parameters), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await clientMock.Object.SendNotification(url, method, parameters, new CancellationToken());

            clientMock.Verify(x => x.SendNotification(url, It.Is<Notification<object>>(y => y.Method == method && y.Params == parameters), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendNotification3_ChainsToActualMehtod()
        {
            var method = "method";
            var parameters = new object();

            clientMock.Setup(x => x.SendNotification(null, It.Is<Notification<object>>(y => y.Method == method && y.Params == parameters), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await clientMock.Object.SendNotification(method, parameters, new CancellationToken());

            clientMock.Verify(x => x.SendNotification(null, It.Is<Notification<object>>(y => y.Method == method && y.Params == parameters), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendRequest1_ChainsToActualMehtod()
        {
            var request = new Request<object>();
            clientMock.Setup(x => x.SendRequest(null, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>());

            await clientMock.Object.SendRequest(request, new CancellationToken());

            clientMock.Verify(x => x.SendRequest(null, request, It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendRequest2_ChainsToActualMehtod()
        {
            var url = "test";
            var method = "method";
            var parameters = new object();
            var id = Mock.Of<IRpcId>();
            clientMock.Setup(x => x.SendRequest(url, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>());

            await clientMock.Object.SendRequest(url, id, method, parameters, new CancellationToken());

            clientMock.Verify(x => x.SendRequest(url, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendRequest3_ChainsToActualMehtod()
        {
            var method = "method";
            var parameters = new object();
            var id = Mock.Of<IRpcId>();
            clientMock.Setup(x => x.SendRequest(null, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>());

            await clientMock.Object.SendRequest(id, method, parameters, new CancellationToken());

            clientMock.Verify(x => x.SendRequest(null, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendRequest4_ChainsToActualMehtod()
        {
            var url = "test";
            var method = "method";
            var parameters = new object();
            var id = Mock.Of<IRpcId>();
            clientMock.Setup(x => x.SendRequest(url, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>());
            generatorMock.Setup(x => x.GenerateId())
                .Returns(id);

            await clientMock.Object.SendRequest(url, method, parameters, new CancellationToken());

            clientMock.Verify(x => x.SendRequest(url, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendRequest5_ChainsToActualMehtod()
        {
            var method = "method";
            var parameters = new object();
            var id = Mock.Of<IRpcId>();
            clientMock.Setup(x => x.SendRequest(null, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>());
            generatorMock.Setup(x => x.GenerateId())
                .Returns(id);

            await clientMock.Object.SendRequest(method, parameters, new CancellationToken());

            clientMock.Verify(x => x.SendRequest(null, It.Is<Request<object>>(y => y.Method == method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendBatch_ChainsToActualMehtod()
        {
            var batch = new List<ICall>();
            clientMock.Setup(x => x.SendBatch(null, batch, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IBatchJsonRpcResult>());

            await clientMock.Object.SendBatch(batch, new CancellationToken());

            clientMock.Verify(x => x.SendBatch(null, batch, It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_Send_ChainsToActualMehtod()
        {
            var request = new Request<object>();
            clientMock.Setup(x => x.Send(null, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<HttpResponseMessage>());

            await clientMock.Object.Send(request, new CancellationToken());

            clientMock.Verify(x => x.Send(null, request, It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendNotification_SerializesContentAndHeadersAndChecksResponse()
        {
            var notification = new Notification<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK);

            await clientMock.Object.SendNotification(notification, new CancellationToken());

            serializerMock.Verify(x => x.Serializer);
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<UntypedNotification>()));
        }

        [TestCase(HttpStatusCode.NoContent)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadRequest)]
        public async Task Test_SendNotification_ThrowsOnBadResponse(HttpStatusCode httpStatusCode)
        {
            var notification = new Notification<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(httpStatusCode);

            Func<Task> action = async () => await clientMock.Object.SendNotification(notification, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();
        }

        [Test]
        public async Task Test_SendRequest_SerializesContentAndHeadersAndReadsResponse()
        {
            var request = new Request<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, "{}");
            clientMock.Setup(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SingleResponseWrapper(){Single = new UntypedResponse()});

            var result = await clientMock.Object.SendRequest(request, new CancellationToken());

            result.Should().NotBeNull();
            serializerMock.Verify(x => x.Serializer);
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<UntypedRequest>()));
            clientMock.Verify(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()));
        }

        [TestCase(HttpStatusCode.NoContent)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadRequest)]
        public async Task Test_SendRequest_ThrowsOnBadResponse(HttpStatusCode httpStatusCode)
        {
            var request = new Request<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(httpStatusCode);

            Func<Task> action = async () => await clientMock.Object.SendRequest(request, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();
        }

        [Test]
        public async Task Test_SendRequest_ThrowsOnNullBody()
        {
            var request = new Request<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(x => new HttpResponseMessage(HttpStatusCode.OK));

            Func<Task> action = async () => await clientMock.Object.SendRequest(request, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();
        }

        [Test]
        public async Task Test_SendRequest_ThrowsOnNullContentLength()
        {
            var request = new Request<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(x => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
                {
                    Headers =
                    {
                        ContentLength = null
                    }
                }
            });

            Func<Task> action = async () => await clientMock.Object.SendRequest(request, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();
        }

        [Test]
        public async Task Test_SendRequest_ThrowsOnEmptyBody()
        {
            var request = new Request<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, string.Empty);

            Func<Task> action = async () => await clientMock.Object.SendRequest(request, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();
        }

        [Test]
        public async Task Test_SendRequest_ThrowsOnBadResponse()
        {
            var request = new Request<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, "{}");
            clientMock.Setup(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IResponseWrapper>());

            Func<Task> action = async () => await clientMock.Object.SendRequest(request, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();
        }

        [Test]
        public async Task Test_SendBatch_SerializesContentAndHeadersAndReadsResponse()
        {
            var batch = new List<ICall>
            {
                new Request<object>()
                {
                    Params = new object()
                },
                new Notification<object>()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, "{}");
            clientMock.Setup(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BatchResponseWrapper() { Batch = new List<IResponse>()
                {
                    new Response<object>()
                } });

            var result = await clientMock.Object.SendBatch(batch, new CancellationToken());

            result.Should().NotBeNull();
            serializerMock.Verify(x => x.Serializer);
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<List<IUntypedCall>>()));
            clientMock.Verify(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendBatch_ThrowsOnSingleResponse()
        {
            var batch = new List<ICall>
            {
                new Request<object>()
                {
                    Params = new object()
                },
                new Notification<object>()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, "{}");
            clientMock.Setup(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SingleResponseWrapper()
                {
                    Single = new Response<object>()
                });
            Func<Task> action = async () => await clientMock.Object.SendBatch(batch, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();

            serializerMock.Verify(x => x.Serializer);
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<List<IUntypedCall>>()));
            clientMock.Verify(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendBatch_ThrowsOnBadResponse()
        {
            var batch = new List<ICall>
            {
                new Request<object>()
                {
                    Params = new object()
                },
                new Notification<object>()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, "{}");
            clientMock.Setup(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IResponseWrapper>());
            Func<Task> action = async () => await clientMock.Object.SendBatch(batch, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();

            serializerMock.Verify(x => x.Serializer);
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<List<IUntypedCall>>()));
            clientMock.Verify(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendBatch_ThrowsOnNullResponse()
        {
            var batch = new List<ICall>
            {
                new Request<object>()
                {
                    Params = new object()
                },
                new Notification<object>()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, "{}");
            clientMock.Setup(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IResponseWrapper)null);
            Func<Task> action = async () => await clientMock.Object.SendBatch(batch, new CancellationToken());

            await action.Should().ThrowAsync<JsonRpcException>();

            serializerMock.Verify(x => x.Serializer);
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<List<IUntypedCall>>()));
            clientMock.Verify(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Test_SendBatch_ReturnsNullWhenNotificationsBatch()
        {
            var batch = new List<ICall>
            {
                new Notification<object>()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(HttpStatusCode.OK, JsonRpcConstants.ContentType, "{}");
            clientMock.Setup(x => x.ParseBody(It.IsAny<HttpResponseMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BatchResponseWrapper()
                {
                    Batch = new List<IResponse>()
                });
            var result = await clientMock.Object.SendBatch(batch, new CancellationToken());

            result.Should().BeNull();
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<List<IUntypedCall>>()));
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.NoContent)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadRequest)]
        public async Task Test_Send_SerializesContentAndReturnsResponse(HttpStatusCode httpStatusCode)
        {
            var request = new Request<object>
            {
                Params = new object()
            };
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            handlerMock.When("*").Respond(httpStatusCode);

            var result = await clientMock.Object.Send(request, new CancellationToken());

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(httpStatusCode);
            serializerMock.Verify(x => x.Serializer);
            clientMock.Verify(x => x.CreateHttpContent(It.IsAny<UntypedRequest>()));
        }

        [Test]
        public async Task Test_ParseBody_DeserializesJson()
        {
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            var httpResponse = new HttpResponseMessage()
            {
                Content = new StringContent(@"{""id"": null, ""result"": null}")
            };

            var result = await clientMock.Object.ParseBody(httpResponse, new CancellationToken());

            result.Should().BeOfType<SingleResponseWrapper>();
        }

        [Test]
        public async Task Test_ParseBody_ThrowsOnBadJson()
        {
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer());
            var httpResponse = new HttpResponseMessage()
            {
                Content = new StringContent(@"{""id"": null, ""result"": null")
            };

            Func<Task> action = async () => await clientMock.Object.ParseBody(httpResponse, new CancellationToken());

            await action.Should().ThrowAsync<JsonException>();
        }
    }
}