using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Features;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Services;

[TestFixture]
internal class JsonRpcRequestHandlerTests
{
    private Mock<IJsonRpcExceptionWrapper> exceptionWrapperMock;
    private JsonRpcServerOptions options;
    private JsonRpcRequestHandler requestHandler;

    [SetUp]
    public void Setup()
    {
        exceptionWrapperMock = new Mock<IJsonRpcExceptionWrapper>();
        options = new JsonRpcServerOptions();

        requestHandler = new JsonRpcRequestHandler(exceptionWrapperMock.Object, Options.Create(options));
    }

    [TestCase("\"\"")]
    [TestCase("\" \"")]
    [TestCase("null")]
    public async Task ProcessJsonRpcRequest_SingleRequestMethodIsEmpty_WrapJsonRpcFormatException(string methodName)
    {
        var id = new NumberRpcId(Id);
        var rawCall = JsonDocument.Parse($$"""
            {
                "id": {{Id}},
                "method": {{methodName}},
                "jsonrpc": "2.0"
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedErrorResponse(id, new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(w => w.WrapGeneralException(It.Is<Exception>(static e => e is JsonRpcFormatException), id))
            .Returns(response)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        exceptionWrapperMock.Verify();
    }

    [TestCase("\"\"")]
    [TestCase("\" \"")]
    [TestCase("null")]
    public async Task ProcessJsonRpcRequest_SingleNotificationMethodIsEmpty_ReturnNull(string methodName)
    {
        var rawCall = JsonDocument.Parse($$"""
            {
                "method": {{methodName}},
                "jsonrpc": "2.0"
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        result.Should().BeNull();
    }

    [TestCase("\"\"")]
    [TestCase("\" \"")]
    [TestCase("\"123\"")]
    [TestCase("\"1.0\"")]
    [TestCase("\"3.0\"")]
    [TestCase("null")]
    public async Task ProcessJsonRpcRequest_SingleRequestUnsupportedVersion_WrapJsonRpcFormatException(string version)
    {
        var id = new NumberRpcId(Id);
        var rawCall = JsonDocument.Parse($$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": {{version}}
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedErrorResponse(id, new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(w => w.WrapGeneralException(It.Is<Exception>(static e => e is JsonRpcFormatException), id))
            .Returns(response)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        exceptionWrapperMock.Verify();
    }

    [TestCase("\"\"")]
    [TestCase("\" \"")]
    [TestCase("\"123\"")]
    [TestCase("\"1.0\"")]
    [TestCase("\"3.0\"")]
    [TestCase("null")]
    public async Task ProcessJsonRpcRequest_SingleNotificationUnsupportedVersion_ReturnNull(string version)
    {
        var rawCall = JsonDocument.Parse($$"""
            {
                "method": "{{MethodName}}",
                "jsonrpc": {{version}}
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        result.Should().BeNull();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_SingleRequestNextThrows_WrapException()
    {
        var id = new NumberRpcId(Id);
        var rawCall = JsonDocument.Parse($$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var exception = new ArgumentException();
        nextMock.Setup(n => n(httpContext))
            .Throws(exception)
            .Verifiable();
        var response = new UntypedErrorResponse(id, new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(w => w.WrapGeneralException(exception, id))
            .Returns(response)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        nextMock.Verify();
        exceptionWrapperMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_SingleNotificationNextThrows_ReturnNull()
    {
        var rawCall = JsonDocument.Parse($$"""
            {
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(httpContext))
            .Throws<ArgumentException>()
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        result.Should().BeNull();
        nextMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_SingleRequestNextRemovesFeature_WrapJsonRpcServerException()
    {
        var id = new NumberRpcId(Id);
        var rawCall = JsonDocument.Parse($$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(httpContext))
            .Callback<HttpContext>(static context => context.Features.Set<IJsonRpcFeature>(null))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var response = new UntypedErrorResponse(id, new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(w => w.WrapGeneralException(It.Is<Exception>(static e => e is JsonRpcServerException), id))
            .Returns(response)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        nextMock.Verify();
        exceptionWrapperMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_SingleNotificationNextRemovesFeature_ReturnNull()
    {
        var rawCall = JsonDocument.Parse($$"""
            {
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(httpContext))
            .Callback<HttpContext>(static context => context.Features.Set<IJsonRpcFeature>(null))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        result.Should().BeNull();
        nextMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_SingleRequestNextSetsResponseInFeature_ReturnResponse()
    {
        var id = new NumberRpcId(Id);
        var rawCall = JsonDocument.Parse($$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedResponse(id, null);
        nextMock.Setup(n => n(httpContext))
            .Callback<HttpContext>(context => context.SetJsonRpcResponse(response))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        nextMock.Verify();
        exceptionWrapperMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequestWithFloatId_SingleRequestNextSetsResponseInFeature_ReturnResponse()
    {
        var floatId = 123.454;
        var id = new FloatNumberRpcId(floatId);
        var rawCall = JsonDocument.Parse($$"""
                                           {
                                               "id": {{floatId.ToString(CultureInfo.InvariantCulture)}},
                                               "method": "{{MethodName}}",
                                               "jsonrpc": "2.0"
                                           }
                                           """);
        var requestWrapper = new SingleRequestWrapper(rawCall);
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedResponse(id, null);
        nextMock.Setup(n => n(httpContext))
            .Callback<HttpContext>(context => context.SetJsonRpcResponse(response))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        nextMock.Verify();
        exceptionWrapperMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_BatchRequestWithZeroCalls_WrapJsonRpcFormatException()
    {
        var requestWrapper = new BatchRequestWrapper(new List<JsonDocument>());
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedErrorResponse(new NullRpcId(), new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(static w => w.WrapGeneralException(It.Is<Exception>(static e => e is JsonRpcFormatException), null))
            .Returns(response)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        exceptionWrapperMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_BatchRequest_ClearEndpointAfterEveryRequest()
    {
        var requestWrapper = new BatchRequestWrapper(new List<JsonDocument>
        {
            JsonDocument.Parse($$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """),
            JsonDocument.Parse($$"""
            {
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """)
        });
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedResponse(new NullRpcId(), null);
        nextMock.Setup(n => n(It.Is<HttpContext>(c => c.GetEndpoint() == null && c == httpContext)))
            .Callback<HttpContext>(c => c.SetJsonRpcResponse(response))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new BatchResponseWrapper(new List<IResponse> { response, response });
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        httpContext.GetEndpoint().Should().BeNull();
        nextMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_BatchRequest_ReturnOnlyRequestResponses()
    {
        var requestWrapper = new BatchRequestWrapper(new List<JsonDocument>
        {
            JsonDocument.Parse($$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """),
            JsonDocument.Parse($$"""
            {
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """)
        });
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedResponse(new NullRpcId(), null);
        nextMock.Setup(n => n(httpContext))
            .Callback<HttpContext>(c =>
            {
                if (c.GetJsonRpcCall() is UntypedRequest)
                {
                    c.SetJsonRpcResponse(response);
                }
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expectedResponseWrapper = new BatchResponseWrapper(new List<IResponse> { response });
        result.Should().BeEquivalentTo(expectedResponseWrapper);
        nextMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_BatchRequestWithNotificationsOnly_ReturnNull()
    {
        var requestWrapper = new BatchRequestWrapper(new List<JsonDocument>
        {
            JsonDocument.Parse($$"""
            {
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """),
            JsonDocument.Parse($$"""
            {
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """)
        });
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(httpContext))
            .Callback<HttpContext>(static c =>
            {
                if (c.GetJsonRpcCall() is UntypedRequest)
                {
                    c.SetJsonRpcResponse(new UntypedResponse(new NullRpcId(), null));
                }
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        result.Should().BeNull();
        nextMock.Verify();
    }

    [Test]
    public async Task ProcessJsonRpcRequest_UnknownWrapper_WrapArgumentOutOfRangeException()
    {
        var requestWrapper = Mock.Of<IRequestWrapper>();
        var httpContext = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        var response = new UntypedErrorResponse(new NullRpcId(), new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(static w => w.WrapGeneralException(It.Is<Exception>(static e => e is ArgumentOutOfRangeException), null))
            .Returns(response)
            .Verifiable();

        var result = await requestHandler.ProcessJsonRpcRequest(requestWrapper, httpContext, nextMock.Object);

        var expected = new SingleResponseWrapper(response);
        result.Should().BeEquivalentTo(expected);
        exceptionWrapperMock.Verify();
    }

    private const int Id = 123;
    private const string MethodName = "methodName";
}
