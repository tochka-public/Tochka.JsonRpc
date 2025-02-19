using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Tests.Models;

[TestFixture]
public class JsonRpcCallContextTests
{
    private JsonRpcCallContext jsonRpcCallContext;

    [SetUp]
    public void Setup() => jsonRpcCallContext = new JsonRpcCallContext();

    [Test]
    public void WithRequestUrl_RequestUrlIsNull_DontThrow()
    {
        var action = () => jsonRpcCallContext.WithRequestUrl(null);

        action.Should().NotThrow();
    }

    [Test]
    public void WithRequestUrl_RequestUrlStartsWithSlash_Throw()
    {
        var action = () => jsonRpcCallContext.WithRequestUrl("/url");

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void WithRequestUrl_ValidRequestUrl_SetProperty()
    {
        var requestUrl = "url";

        jsonRpcCallContext.WithRequestUrl(requestUrl);

        jsonRpcCallContext.RequestUrl.Should().Be(requestUrl);
    }

    [Test]
    public void WithSingle_BatchCallNotNull_Throw()
    {
        jsonRpcCallContext.WithBatch(new List<IUntypedCall>());

        var action = () => jsonRpcCallContext.WithSingle(Mock.Of<IUntypedCall>());

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void WithSingle_BatchCallIsNull_SetProperty()
    {
        var call = new UntypedNotification("method", null);

        jsonRpcCallContext.WithSingle(call);

        jsonRpcCallContext.SingleCall.Should().BeEquivalentTo(call);
    }

    [Test]
    public void WithBatch_SingleCallNotNull_Throw()
    {
        jsonRpcCallContext.WithSingle(Mock.Of<IUntypedCall>());

        var action = () => jsonRpcCallContext.WithBatch(new List<IUntypedCall>());

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void WithBatch_SingleCallIsNull_SetProperties()
    {
        var calls = new List<IUntypedCall>
        {
            new UntypedNotification("method", null),
            new UntypedRequest(new NullRpcId(), "method", null),
            new UntypedRequest(new NullRpcId(), "method", null)
        };

        jsonRpcCallContext.WithBatch(calls);

        jsonRpcCallContext.BatchCall.Should().BeEquivalentTo(calls);
        jsonRpcCallContext.ExpectedBatchResponseCount.Should().Be(2);
    }

    [Test]
    public void WithHttpResponse_StatusOk_SetProperty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        jsonRpcCallContext.WithHttpResponse(response);

        jsonRpcCallContext.HttpResponseInfo.Should().Be($"{response}");
    }

    [TestCase(HttpStatusCode.Continue)]
    [TestCase(HttpStatusCode.Created)]
    [TestCase(HttpStatusCode.Ambiguous)]
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void WithHttpResponse_StatusNotOk_Throw(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode);

        var action = () => jsonRpcCallContext.WithHttpResponse(response);

        action.Should().Throw<JsonRpcException>();
        jsonRpcCallContext.HttpResponseInfo.Should().Be($"{response}");
    }

    [Test]
    public void WithSingleResponse_SetProperty()
    {
        var id = new NullRpcId();
        var response = new UntypedResponse(id, null);
        jsonRpcCallContext.WithSingle(new UntypedRequest(id, "method", null));

        jsonRpcCallContext.WithSingleResponse(response);

        jsonRpcCallContext.SingleResponse.Should().BeEquivalentTo(response);
    }

    [Test]
    public void WithSingleResponse_BatchResponseNotNull_Throw()
    {
        var id = new NullRpcId();
        var response = new UntypedResponse(id, null);
        jsonRpcCallContext.WithBatch(new[] { new UntypedRequest(id, "method", null) });
        jsonRpcCallContext.WithBatchResponse(new List<IResponse> { response });

        var action = () => jsonRpcCallContext.WithSingleResponse(response);

        action.Should().Throw<InvalidOperationException>();
        jsonRpcCallContext.SingleResponse.Should().BeEquivalentTo(response);
    }

    [Test]
    public void WithSingleResponse_ResponseIsNull_Throw()
    {
        var id = new NullRpcId();
        jsonRpcCallContext.WithSingle(new UntypedRequest(id, "method", null));

        var action = () => jsonRpcCallContext.WithSingleResponse(null);

        action.Should().Throw<JsonRpcException>();
    }

    [Test]
    public void WithSingleResponse_InvalidVersion_Throw()
    {
        var id = new NullRpcId();
        var response = new UntypedResponse(id, null, "1.0");
        jsonRpcCallContext.WithSingle(new UntypedRequest(id, "method", null));

        var action = () => jsonRpcCallContext.WithSingleResponse(response);

        action.Should().Throw<JsonRpcException>();
        jsonRpcCallContext.SingleResponse.Should().BeEquivalentTo(response);
    }

    [Test]
    public void WithSingleResponse_BatchCallNotNull_DontThrow()
    {
        var id = new NullRpcId();
        var response = new UntypedResponse(id, null);
        jsonRpcCallContext.WithBatch(new[] { new UntypedRequest(id, "method", null) });

        var action = () => jsonRpcCallContext.WithSingleResponse(response);

        action.Should().NotThrow();
        jsonRpcCallContext.SingleResponse.Should().BeEquivalentTo(response);
    }

    [Test]
    public void WithSingleResponse_SingleCallNotRequest_Throw()
    {
        var id = new NullRpcId();
        var response = new UntypedResponse(id, null);
        jsonRpcCallContext.WithSingle(new UntypedNotification("method", null));

        var action = () => jsonRpcCallContext.WithSingleResponse(response);

        action.Should().Throw<JsonRpcException>();
        jsonRpcCallContext.SingleResponse.Should().BeEquivalentTo(response);
    }

    [Test]
    public void WithSingleResponse_ErrorResponseIdIsNull_DontThrow()
    {
        var id = new StringRpcId("123");
        var response = new UntypedErrorResponse(new NullRpcId(), new Error<JsonDocument>(1, "message", null));
        jsonRpcCallContext.WithSingle(new UntypedRequest(id, "method", null));

        var action = () => jsonRpcCallContext.WithSingleResponse(response);

        action.Should().NotThrow();
        jsonRpcCallContext.SingleResponse.Should().BeEquivalentTo(response);
    }

    [Test]
    public void WithSingleResponse_ResponseIdNotNullAndDifferent_DontThrow()
    {
        var response = new UntypedResponse(new StringRpcId("123"), null);
        jsonRpcCallContext.WithSingle(new UntypedRequest(new StringRpcId("456"), "method", null));

        var action = () => jsonRpcCallContext.WithSingleResponse(response);

        action.Should().Throw<JsonRpcException>();
        jsonRpcCallContext.SingleResponse.Should().BeEquivalentTo(response);
    }

    [Test]
    public void WithBatchResponse_SetProperty()
    {
        var id = new NullRpcId();
        var responses = new List<IResponse> { new UntypedResponse(id, null) };
        jsonRpcCallContext.WithBatch(new[] { new UntypedRequest(id, "method", null) });

        jsonRpcCallContext.WithBatchResponse(responses);

        jsonRpcCallContext.BatchResponse.Should().BeEquivalentTo(responses);
    }

    [Test]
    public void WithBatchResponse_SingleResponseNotNull_Throw()
    {
        var id = new NullRpcId();
        var responses = new List<IResponse> { new UntypedResponse(id, null) };
        jsonRpcCallContext.WithSingle(new UntypedRequest(id, "method", null));
        jsonRpcCallContext.WithSingleResponse(new UntypedResponse(id, null));

        var action = () => jsonRpcCallContext.WithBatchResponse(responses);

        action.Should().Throw<InvalidOperationException>();
        jsonRpcCallContext.BatchResponse.Should().BeEquivalentTo(responses);
    }

    [Test]
    public void WithBatchResponse_ExpectedBatchResponseCountIsZero_Throw()
    {
        var id = new NullRpcId();
        var responses = new List<IResponse> { new UntypedResponse(id, null) };
        jsonRpcCallContext.WithBatch(new[] { new UntypedNotification("method", null) });

        var action = () => jsonRpcCallContext.WithBatchResponse(responses);

        action.Should().Throw<InvalidOperationException>();
        jsonRpcCallContext.BatchResponse.Should().BeEquivalentTo(responses);
    }

    [Test]
    public void WithBatchResponse_BatchResponseIsNull_Throw()
    {
        var id = new NullRpcId();
        jsonRpcCallContext.WithBatch(new[] { new UntypedRequest(id, "method", null) });

        var action = () => jsonRpcCallContext.WithBatchResponse(null);

        action.Should().Throw<JsonRpcException>();
    }

    [Test]
    public void WithBatchResponse_BatchResponseIsEmpty_Throw()
    {
        var id = new NullRpcId();
        var responses = new List<IResponse>();
        jsonRpcCallContext.WithBatch(new[] { new UntypedRequest(id, "method", null) });

        var action = () => jsonRpcCallContext.WithBatchResponse(responses);

        action.Should().Throw<JsonRpcException>();
        jsonRpcCallContext.BatchResponse.Should().BeEquivalentTo(responses);
    }

    [Test]
    public void WithBatchResponse_ExpectedAndActualCountNotEqual_Throw()
    {
        var id = new NullRpcId();
        var responses = new List<IResponse> { new UntypedResponse(id, null) };
        jsonRpcCallContext.WithBatch(new[]
        {
            new UntypedRequest(id, "method", null),
            new UntypedRequest(id, "method", null)
        });

        var action = () => jsonRpcCallContext.WithBatchResponse(responses);

        action.Should().Throw<JsonRpcException>();
        jsonRpcCallContext.BatchResponse.Should().BeEquivalentTo(responses);
    }

    [Test]
    public void WithBatchResponse_InvalidVersion_Throw()
    {
        var id = new NullRpcId();
        var responses = new List<IResponse> { new UntypedResponse(id, null, "1.0") };
        jsonRpcCallContext.WithBatch(new[] { new UntypedRequest(id, "method", null) });

        var action = () => jsonRpcCallContext.WithBatchResponse(responses);

        action.Should().Throw<JsonRpcException>();
        jsonRpcCallContext.BatchResponse.Should().BeEquivalentTo(responses);
    }

    [Test]
    public void WithError_SetProperty()
    {
        var error = new Error<JsonDocument>(1, "message", null);

        jsonRpcCallContext.WithError(new UntypedErrorResponse(new NullRpcId(), error));

        jsonRpcCallContext.Error.Should().BeEquivalentTo(error);
    }
}
