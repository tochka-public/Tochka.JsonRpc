using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Tests
{
    public class JsonRpcCallContextTests
    {
        private JsonRpcCallContext jsonRpcCallContext;

        [SetUp]
        public void Setup()
        {
            jsonRpcCallContext = new JsonRpcCallContext();
        }

        [Test]
        public void Test_WithRequestUrl_ThrowsOnLeadingSlash()
        {
            Action action = () => jsonRpcCallContext.WithRequestUrl("/test");

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Test_WithRequestUrl_SetsValidUrl()
        {
            var requestUrl = "test";

            jsonRpcCallContext.WithRequestUrl(requestUrl);

            jsonRpcCallContext.RequestUrl.Should().Be(requestUrl);
        }

        [Test]
        public void Test_WithSingleResponse_ThrowsIfHasBatchResponse()
        {
            jsonRpcCallContext.WithBatch(new List<IUntypedCall>() { new UntypedRequest()});
            jsonRpcCallContext.WithBatchResponse(new List<IResponse>(){new UntypedResponse()});

            Action action = () => jsonRpcCallContext.WithSingleResponse(Mock.Of<IResponse>());

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_WithSingleResponse_ThrowsOnNull()
        {
            Action action = () => jsonRpcCallContext.WithSingleResponse(null);

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithSingleResponse_ThrowsOnWrongVersion()
        {
            Action action = () => jsonRpcCallContext.WithSingleResponse(new UntypedResponse(){Jsonrpc = ""});

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithSingleResponse_ThrowsOnNotification()
        {
            jsonRpcCallContext.WithSingle(new UntypedNotification());

            Action action = () => jsonRpcCallContext.WithSingleResponse(new UntypedResponse());

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithSingleResponse_ThrowsOnNullRequest()
        {
            Action action = () => jsonRpcCallContext.WithSingleResponse(new UntypedResponse());

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithSingleResponse_ThrowsOnIdMismatch()
        {
            jsonRpcCallContext.WithSingle(new UntypedRequest(){Id = new StringRpcId("")});

            Action action = () => jsonRpcCallContext.WithSingleResponse(new UntypedResponse());

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithSingleResponse_Works()
        {
            jsonRpcCallContext.WithSingle(new UntypedRequest());
            var response = new UntypedResponse();

            Action action = () => jsonRpcCallContext.WithSingleResponse(response);

            action.Should().NotThrow();
            jsonRpcCallContext.SingleResponse.Should().Be(response);
        }

        [Test]
        public void Test_WithBatchResponse_ThrowsIfHasSingleResponse()
        {
            jsonRpcCallContext.WithSingle(new UntypedRequest());
            jsonRpcCallContext.WithSingleResponse(new UntypedResponse());
            
            Action action = () => jsonRpcCallContext.WithBatchResponse(new List<IResponse>() { new UntypedResponse() });

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_WithBatchResponse_ThrowsIfNotExpectedAnyResponse()
        {
            jsonRpcCallContext.WithBatch(new List<IUntypedCall>());

            Action action = () => jsonRpcCallContext.WithBatchResponse(new List<IResponse>() { new UntypedResponse() });

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_WithBatchResponse_ThrowsOnNull()
        {
            jsonRpcCallContext.WithBatch(new List<IUntypedCall>(){new UntypedRequest()});

            Action action = () => jsonRpcCallContext.WithBatchResponse(null);

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithBatchResponse_ThrowsOnEmpty()
        {
            jsonRpcCallContext.WithBatch(new List<IUntypedCall>() { new UntypedRequest() });

            Action action = () => jsonRpcCallContext.WithBatchResponse(new List<IResponse>());

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithBatchResponse_ThrowsOnCountMismatch()
        {
            jsonRpcCallContext.WithBatch(new List<IUntypedCall>() { new UntypedRequest(), new UntypedRequest() });

            Action action = () => jsonRpcCallContext.WithBatchResponse(new List<IResponse>(){new UntypedResponse()});

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithBatchResponse_ThrowsOnWrongVersion()
        {
            jsonRpcCallContext.WithBatch(new List<IUntypedCall>() { new UntypedRequest(), new UntypedRequest() });

            Action action = () => jsonRpcCallContext.WithBatchResponse(new List<IResponse>() { new UntypedResponse(), new UntypedResponse(){Jsonrpc = ""} });

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithBatchResponse_Works()
        {
            jsonRpcCallContext.WithBatch(new List<IUntypedCall>() { new UntypedRequest(), new UntypedRequest() });
            var batchResponse = new List<IResponse>() { new UntypedResponse(), new UntypedResponse() };

            Action action = () => jsonRpcCallContext.WithBatchResponse(batchResponse);

            action.Should().NotThrow();

            Assert.AreEqual(batchResponse, jsonRpcCallContext.BatchResponse);
        }

        [Test]
        public void Test_WithError_Works()
        {
            var error = Mock.Of<IError>();

            jsonRpcCallContext.WithError(error);

            jsonRpcCallContext.Error.Should().Be(error);
        }

        [TestCase(HttpStatusCode.NoContent)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadRequest)]
        public void Test_WithHttpResponse_ThrowsOnBadCode(HttpStatusCode httpStatusCode)
        {
            Action action = () => jsonRpcCallContext.WithHttpResponse(new HttpResponseMessage(httpStatusCode));

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithHttpResponse_Works()
        {
            Action action = () => jsonRpcCallContext.WithHttpResponse(new HttpResponseMessage(HttpStatusCode.OK));

            action.Should().NotThrow();
            jsonRpcCallContext.HttpResponseInfo.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void Test_WithHttpContent_ThrowsOnNull()
        {
            Action action = () => jsonRpcCallContext.WithHttpContent(null);

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithHttpContent_ThrowsOnNullContentLength()
        {
            var content = new StringContent(string.Empty)
            {
                Headers =
                {
                    ContentLength = null
                }
            };

            Action action = () => jsonRpcCallContext.WithHttpContent(content);

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithHttpContent_ThrowsOnZeroContentLength()
        {
            var content = new StringContent(string.Empty);

            Action action = () => jsonRpcCallContext.WithHttpContent(content);

            action.Should().Throw<JsonRpcException>();
        }

        [Test]
        public void Test_WithHttpContent_Works()
        {
            var content = new StringContent("test");

            Action action = () => jsonRpcCallContext.WithHttpContent(content);

            action.Should().NotThrow();
            jsonRpcCallContext.HttpContentInfo.Should().NotBeNullOrWhiteSpace();
        }
    }
}