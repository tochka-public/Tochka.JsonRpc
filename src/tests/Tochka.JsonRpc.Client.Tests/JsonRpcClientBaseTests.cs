using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Client.Tests;

[TestFixture]
internal class JsonRpcClientBaseTests
{
    private Mock<JsonRpcClientBase> clientMock;
    private MockHttpMessageHandler handlerMock;
    private Mock<IJsonRpcIdGenerator> generatorMock;

    [SetUp]
    public void Setup()
    {
        handlerMock = new MockHttpMessageHandler();
        generatorMock = new Mock<IJsonRpcIdGenerator>();
        var httpClient = handlerMock.ToHttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        clientMock = new Mock<JsonRpcClientBase>(httpClient, generatorMock.Object, Mock.Of<ILogger>())
        {
            CallBase = true
        };
    }

    [Test]
    public void Ctor_InitializeHttpClient()
    {
        // Needed to call constructor
        var client = clientMock.Object;

        var httpClient = client.Client;
        httpClient.DefaultRequestHeaders.Should().ContainKey("User-Agent");
        httpClient.DefaultRequestHeaders.UserAgent.ToString().Should().Be(client.UserAgent);
    }

    [Test]
    public async Task SendNotification1_ChainsToInternalMethod()
    {
        var notification = new Notification<object>(Method, null);
        clientMock.Setup(x => x.SendNotificationInternal(RequestUrl, notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(RequestUrl, notification, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendNotification2_ChainsToInternalMethod()
    {
        var notification = new Notification<object>(Method, null);
        clientMock.Setup(x => x.SendNotificationInternal(null, notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(notification, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendNotification3_ChainsToInternalMethod()
    {
        var parameters = new object();
        clientMock.Setup(x => x.SendNotificationInternal(RequestUrl, It.Is<Notification<object>>(y => y.Method == Method && y.Params == parameters), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(RequestUrl, Method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendNotification4_ChainsToInternalMethod()
    {
        var parameters = new object();
        clientMock.Setup(x => x.SendNotificationInternal(null, It.Is<Notification<object>>(y => y.Method == Method && y.Params == parameters), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await clientMock.Object.SendNotification(Method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendRequest1_ChainsToInternalMethod()
    {
        var request = new Request<object>(new NullRpcId(), Method, null);
        clientMock.Setup(x => x.SendRequestInternal(RequestUrl, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(RequestUrl, request, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendRequest2_ChainsToInternalMethod()
    {
        var request = new Request<object>(new NullRpcId(), Method, null);
        clientMock.Setup(x => x.SendRequestInternal(null, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(request, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendRequest3_ChainsToInternalMethod()
    {
        var id = Mock.Of<IRpcId>();
        generatorMock.Setup(static g => g.GenerateId())
            .Returns(id)
            .Verifiable();
        var parameters = new object();
        clientMock.Setup(x => x.SendRequestInternal(RequestUrl, It.Is<Request<object>>(y => y.Method == Method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(RequestUrl, Method, parameters, new CancellationToken());

        generatorMock.Verify();
        clientMock.Verify();
    }

    [Test]
    public async Task SendRequest4_ChainsToInternalMethod()
    {
        var id = Mock.Of<IRpcId>();
        generatorMock.Setup(static g => g.GenerateId())
            .Returns(id)
            .Verifiable();
        var parameters = new object();
        clientMock.Setup(x => x.SendRequestInternal(null, It.Is<Request<object>>(y => y.Method == Method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(Method, parameters, new CancellationToken());

        generatorMock.Verify();
        clientMock.Verify();
    }

    [Test]
    public async Task SendRequest5_ChainsToInternalMethod()
    {
        var id = Mock.Of<IRpcId>();
        var parameters = new object();
        clientMock.Setup(x => x.SendRequestInternal(RequestUrl, It.Is<Request<object>>(y => y.Method == Method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(RequestUrl, id, Method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendRequest6_ChainsToInternalMethod()
    {
        var id = Mock.Of<IRpcId>();
        var parameters = new object();
        clientMock.Setup(x => x.SendRequestInternal(null, It.Is<Request<object>>(y => y.Method == Method && y.Params == parameters && y.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ISingleJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendRequest(id, Method, parameters, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendBatch1_ChainsToInternalMethod()
    {
        var batch = Array.Empty<ICall>();
        clientMock.Setup(x => x.SendBatchInternal(RequestUrl, batch, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IBatchJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendBatch(RequestUrl, batch, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendBatch2_ChainsToInternalMethod()
    {
        var batch = Array.Empty<ICall>();
        clientMock.Setup(x => x.SendBatchInternal(null, batch, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IBatchJsonRpcResult>())
            .Verifiable();

        await clientMock.Object.SendBatch(batch, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Send1_ChainsToInternalMethod()
    {
        var call = new Notification<object>(Method, null);
        clientMock.Setup(x => x.SendInternal(RequestUrl, call, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<HttpResponseMessage>())
            .Verifiable();

        await clientMock.Object.Send(RequestUrl, call, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Send2_ChainsToInternalMethod()
    {
        var call = new Notification<object>(Method, null);
        clientMock.Setup(x => x.SendInternal(null, call, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<HttpResponseMessage>())
            .Verifiable();

        await clientMock.Object.Send(call, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Send3_ChainsToInternalMethod()
    {
        var calls = Array.Empty<ICall>();
        clientMock.Setup(x => x.SendInternal(RequestUrl, calls, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<HttpResponseMessage>())
            .Verifiable();

        await clientMock.Object.Send(RequestUrl, calls, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task Send4_ChainsToInternalMethod()
    {
        var calls = Array.Empty<ICall>();
        clientMock.Setup(x => x.SendInternal(null, calls, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<HttpResponseMessage>())
            .Verifiable();

        await clientMock.Object.Send(calls, new CancellationToken());

        clientMock.Verify();
    }

    [Test]
    public async Task SendNotificationInternal_PostContentToRequestUrl()
    {
        var notification = new Notification<object>(Method, new object());
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        await clientMock.Object.SendNotificationInternal(RequestUrl, notification, CancellationToken.None);

        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task SendNotificationInternal_FillContext()
    {
        var notification = new Notification<object>(Method, new object());
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));

        await clientMock.Object.SendNotificationInternal(RequestUrl, notification, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithSingle(It.IsAny<IUntypedCall>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
    }

    [Test]
    public async Task SendRequestInternal_PostContentToRequestUrlAndParseResponse()
    {
        var request = new Request<object>(new NullRpcId(), Method, new object());
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(Mock.Of<IJsonRpcCallContext>());
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(ResponseContent))
            .Returns(new SingleResponseWrapper(Mock.Of<IResponse>()))
            .Verifiable();

        await clientMock.Object.SendRequestInternal(RequestUrl, request, CancellationToken.None);

        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
        clientMock.Verify();
    }

    [Test]
    public async Task SendRequestInternal_FillContext()
    {
        var request = new Request<object>(new NullRpcId(), Method, new object());
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        var singleResponse = Mock.Of<IResponse>();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(ResponseContent))
            .Returns(new SingleResponseWrapper(singleResponse))
            .Verifiable();

        await clientMock.Object.SendRequestInternal(RequestUrl, request, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithSingle(It.IsAny<IUntypedCall>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
        contextMock.Verify(c => c.WithHttpContent(response.Content, ResponseContent));
        contextMock.Verify(c => c.WithSingleResponse(singleResponse));
    }

    [Test]
    public async Task SendRequestInternal_ThrowOnBatchResponse()
    {
        var request = new Request<object>(new NullRpcId(), Method, new object());
        var contextMock = new Mock<IJsonRpcCallContext>();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(ResponseContent))
            .Returns(new BatchResponseWrapper(new List<IResponse>()))
            .Verifiable();

        var act = async () => await clientMock.Object.SendRequestInternal(RequestUrl, request, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        clientMock.Verify();
    }

    [Test]
    public async Task SendBatchInternal_BatchWithoutRequests_PostContentToRequestUrl()
    {
        var batch = new List<ICall> { new Notification<object>(Method, null) };
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        var response = await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        response.Should().BeNull();
        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task SendBatchInternal_BatchWithoutRequests_FillContext()
    {
        var batch = new List<ICall> { new Notification<object>(Method, null) };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));

        await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithBatch(It.IsAny<ICollection<IUntypedCall>>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
    }

    [Test]
    public async Task SendBatchInternal_BatchWithRequests_PostContentToRequestUrlAndParseResponse()
    {
        var batch = new List<ICall> { new Request<object>(new NullRpcId(), Method, null) };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var batchResponse = new List<IResponse> { Mock.Of<IResponse>() };
        contextMock.Setup(static c => c.ExpectedBatchResponseCount)
            .Returns(batch.Count);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(ResponseContent))
            .Returns(new BatchResponseWrapper(batchResponse))
            .Verifiable();

        var response = await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        response.Should().NotBeNull();
        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
        clientMock.Verify();
    }

    [Test]
    public async Task SendBatchInternal_BatchWithRequests_FillContext()
    {
        var batch = new List<ICall> { new Request<object>(new NullRpcId(), Method, null) };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var response = new HttpResponseMessage();
        var batchResponse = new List<IResponse> { Mock.Of<IResponse>() };
        contextMock.Setup(static c => c.ExpectedBatchResponseCount)
            .Returns(batch.Count);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.When(PostUrl)
            .Respond(() => Task.FromResult(response));
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(ResponseContent))
            .Returns(new BatchResponseWrapper(batchResponse))
            .Verifiable();

        await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        contextMock.Verify(static c => c.WithRequestUrl(RequestUrl));
        contextMock.Verify(static c => c.WithBatch(It.IsAny<ICollection<IUntypedCall>>()));
        contextMock.Verify(c => c.WithHttpResponse(response));
        contextMock.Verify(c => c.WithHttpContent(response.Content, ResponseContent));
        contextMock.Verify(c => c.WithBatchResponse(batchResponse));
    }

    [Test]
    public async Task SendBatchInternal_BatchWithRequestsAndSingleResponse_ThrowAndFillContext()
    {
        var batch = new List<ICall> { new Request<object>(new NullRpcId(), Method, null) };
        var contextMock = new Mock<IJsonRpcCallContext>();
        var singleResponse = Mock.Of<IResponse>();
        contextMock.Setup(static c => c.ExpectedBatchResponseCount)
            .Returns(batch.Count);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(ResponseContent))
            .Returns(new SingleResponseWrapper(singleResponse))
            .Verifiable();

        var act = async () => await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        clientMock.Verify();
        contextMock.Verify(c => c.WithSingleResponse(singleResponse));
    }

    [Test]
    public async Task SendBatchInternal_BatchWithRequestsAndUnknownResponse_Throw()
    {
        var batch = new List<ICall> { new Request<object>(new NullRpcId(), Method, null) };
        var contextMock = new Mock<IJsonRpcCallContext>();
        contextMock.Setup(static c => c.ExpectedBatchResponseCount)
            .Returns(1);
        clientMock.Setup(static c => c.CreateContext())
            .Returns(contextMock.Object);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);
        clientMock.Setup(static c => c.GetContent(It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseContent)
            .Verifiable();
        clientMock.Setup(static c => c.ParseBody(ResponseContent))
            .Returns(Mock.Of<IResponseWrapper>())
            .Verifiable();

        var act = async () => await clientMock.Object.SendBatchInternal(RequestUrl, batch, CancellationToken.None);

        await act.Should().ThrowAsync<JsonRpcException>();
        clientMock.Verify();
    }

    [Test]
    public async Task SendInternal_SingleCall_PostContentToRequestUrl()
    {
        var call = Mock.Of<ICall>();
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        await clientMock.Object.SendInternal(RequestUrl, call, CancellationToken.None);

        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task SendInternal_BatchCall_PostContentToRequestUrl()
    {
        var calls = Array.Empty<ICall>();
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        await clientMock.Object.SendInternal(RequestUrl, calls, CancellationToken.None);

        handlerMock.VerifyNoOutstandingRequest();
        handlerMock.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task SendBatchInternal_BatchWithRequests_Check_HttpNameOptions_Set()
    {
        var batch = new List<ICall> { new Request<object>(new NullRpcId(), Method, null), new Request<object>(new NullRpcId(), OtherMethod, null) };
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        var response = await clientMock.Object.SendInternal(RequestUrl, batch, CancellationToken.None);

        response.RequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<string[]>(JsonRpcConstants.OutgoingHttpRequestOptionMethodNameKey), out var methodNames);
        methodNames.Length.Should().Be(2);
        methodNames[0].Should().Be(Method);
        methodNames[1].Should().Be(OtherMethod);
    }

    [Test]
    public async Task Send_ChainsToInternalMethodCheck_HttpNameOptions_Set()
    {
        var call = new Notification<object>(Method, null);
        handlerMock.Expect(HttpMethod.Post, PostUrl)
            .Respond(HttpStatusCode.OK);

        var response = await clientMock.Object.SendInternal(RequestUrl, call, new CancellationToken());

        response.RequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<string[]>(JsonRpcConstants.OutgoingHttpRequestOptionMethodNameKey), out var methodNames);
        methodNames.Length.Should().Be(1);
        methodNames[0].Should().Be(Method);
    }

    private const string BaseUrl = "https://localhost/";
    private const string Method = "method";
    private const string OtherMethod = "other_method";
    private const string RequestUrl = "request-url";
    private const string PostUrl = $"{BaseUrl}{RequestUrl}";
    private const string ResponseContent = "response-content";
}
