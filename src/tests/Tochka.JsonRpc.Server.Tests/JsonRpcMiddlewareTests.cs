using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests;

[TestFixture]
internal class JsonRpcMiddlewareTests
{
    private Mock<RequestDelegate> nextMock;
    private Mock<IJsonRpcRequestHandler> requestHandlerMock;
    private Mock<IJsonRpcExceptionWrapper> exceptionWrapperMock;
    private JsonRpcServerOptions options;
    private JsonRpcMiddleware middleware;

    [SetUp]
    public void Setup()
    {
        nextMock = new Mock<RequestDelegate>();
        requestHandlerMock = new Mock<IJsonRpcRequestHandler>();
        exceptionWrapperMock = new Mock<IJsonRpcExceptionWrapper>();
        options = new JsonRpcServerOptions();

        middleware = new JsonRpcMiddleware(nextMock.Object, requestHandlerMock.Object, exceptionWrapperMock.Object, Options.Create(options));
    }

    [Test]
    public async Task InvokeAsync_PathDoesntStartWithRoutePrefix_CallNext()
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = "/some/path"
            }
        };

        await middleware.InvokeAsync(httpContext);

        nextMock.Verify(n => n(httpContext));
    }

    [TestCase("GET")]
    [TestCase("PUT")]
    [TestCase("PATCH")]
    [TestCase("DELETE")]
    [TestCase("HEAD")]
    [TestCase("OPTIONS")]
    [TestCase("something")]
    public async Task InvokeAsync_HttpMethodNotPost_CallNext(string httpMethod)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = httpMethod
            }
        };

        await middleware.InvokeAsync(httpContext);

        nextMock.Verify(n => n(httpContext));
    }

    [Test]
    public async Task InvokeAsync_ContentTypeIsNull_CallNext()
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = "POST",
                Headers =
                {
                    ContentType = StringValues.Empty // it will set typed header to null
                }
            }
        };

        await middleware.InvokeAsync(httpContext);

        nextMock.Verify(n => n(httpContext));
    }

    [TestCase(MediaTypeNames.Text.Plain)]
    [TestCase(MediaTypeNames.Text.Html)]
    [TestCase(MediaTypeNames.Application.Xml)]
    [TestCase(MediaTypeNames.Application.Octet)]
    [TestCase(MediaTypeNames.Image.Jpeg)]
    public async Task InvokeAsync_MediaTypeNotJson_CallNext(string contentType)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = "POST",
                Headers =
                {
                    ContentType = contentType
                }
            }
        };

        await middleware.InvokeAsync(httpContext);

        nextMock.Verify(n => n(httpContext));
    }

    [Test]
    public async Task InvokeAsync_EncodingIsNull_UseUTF8()
    {
        var json = $$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """;
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = "POST",
                Headers =
                {
                    ContentType = MediaTypeNames.Application.Json
                },
                Body = new MemoryStream(Encoding.UTF8.GetBytes(json))
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        var responseWrapper = new SingleResponseWrapper(new UntypedResponse(new NumberRpcId(Id), null));
        requestHandlerMock.Setup(h => h.ProcessJsonRpcRequest(It.IsAny<SingleRequestWrapper>(), httpContext, nextMock.Object))
            .ReturnsAsync(responseWrapper)
            .Verifiable();

        await middleware.InvokeAsync(httpContext);

        nextMock.VerifyNoOtherCalls();
        requestHandlerMock.Verify();
        httpContext.Response.ContentType.Should().Be($"{MediaTypeNames.Application.Json}; charset=utf-8");
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var response = await reader.ReadToEndAsync();
        response.Should().Be(JsonSerializer.Serialize(responseWrapper, typeof(IResponseWrapper), options.HeadersJsonSerializerOptions));
    }

    [Test]
    public async Task InvokeAsync_EncodingSet_UseEncoding()
    {
        var json = $$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """;
        var encoding = "utf-32";
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = "POST",
                Headers =
                {
                    ContentType = $"{MediaTypeNames.Application.Json}; charset={encoding}"
                },
                Body = new MemoryStream(Encoding.GetEncoding(encoding).GetBytes(json))
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        var responseWrapper = new SingleResponseWrapper(new UntypedResponse(new NumberRpcId(Id), null));
        requestHandlerMock.Setup(h => h.ProcessJsonRpcRequest(It.IsAny<SingleRequestWrapper>(), httpContext, nextMock.Object))
            .ReturnsAsync(responseWrapper)
            .Verifiable();

        await middleware.InvokeAsync(httpContext);

        nextMock.VerifyNoOtherCalls();
        requestHandlerMock.Verify();
        httpContext.Response.ContentType.Should().Be($"{MediaTypeNames.Application.Json}; charset={encoding}");
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.GetEncoding(encoding));
        var response = await reader.ReadToEndAsync();
        response.Should().Be(JsonSerializer.Serialize(responseWrapper, typeof(IResponseWrapper), options.HeadersJsonSerializerOptions));
    }

    [Test]
    public async Task InvokeAsync_JsonExceptionDuringProcessing_WrapParseException()
    {
        var json = $$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """;
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = "POST",
                Headers =
                {
                    ContentType = $"{MediaTypeNames.Application.Json}"
                },
                Body = new MemoryStream(Encoding.UTF8.GetBytes(json))
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        var exception = new JsonException();
        requestHandlerMock.Setup(h => h.ProcessJsonRpcRequest(It.IsAny<SingleRequestWrapper>(), httpContext, nextMock.Object))
            .ThrowsAsync(exception)
            .Verifiable();
        var errorResponse = new UntypedErrorResponse(new NumberRpcId(Id), new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(w => w.WrapParseException(exception))
            .Returns(errorResponse)
            .Verifiable();

        await middleware.InvokeAsync(httpContext);

        nextMock.VerifyNoOtherCalls();
        requestHandlerMock.Verify();
        exceptionWrapperMock.Verify();
        httpContext.Response.ContentType.Should().Be($"{MediaTypeNames.Application.Json}; charset=utf-8");
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var response = await reader.ReadToEndAsync();
        response.Should().Be(JsonSerializer.Serialize(new SingleResponseWrapper(errorResponse), typeof(IResponseWrapper), options.HeadersJsonSerializerOptions));
    }

    [Test]
    public async Task InvokeAsync_ExceptionDuringProcessing_WrapGeneralException()
    {
        var json = $$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """;
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = "POST",
                Headers =
                {
                    ContentType = $"{MediaTypeNames.Application.Json}"
                },
                Body = new MemoryStream(Encoding.UTF8.GetBytes(json))
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        var exception = new ArgumentException();
        requestHandlerMock.Setup(h => h.ProcessJsonRpcRequest(It.IsAny<SingleRequestWrapper>(), httpContext, nextMock.Object))
            .ThrowsAsync(exception)
            .Verifiable();
        var errorResponse = new UntypedErrorResponse(new NumberRpcId(Id), new Error<JsonDocument>(0, "message", null));
        exceptionWrapperMock.Setup(w => w.WrapGeneralException(exception, null))
            .Returns(errorResponse)
            .Verifiable();

        await middleware.InvokeAsync(httpContext);

        nextMock.VerifyNoOtherCalls();
        requestHandlerMock.Verify();
        exceptionWrapperMock.Verify();
        httpContext.Response.ContentType.Should().Be($"{MediaTypeNames.Application.Json}; charset=utf-8");
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var response = await reader.ReadToEndAsync();
        response.Should().Be(JsonSerializer.Serialize(new SingleResponseWrapper(errorResponse), typeof(IResponseWrapper), options.HeadersJsonSerializerOptions));
    }

    [Test]
    public async Task InvokeAsync_NullResult_DontSetResponse()
    {
        var json = $$"""
            {
                "id": {{Id}},
                "method": "{{MethodName}}",
                "jsonrpc": "2.0"
            }
            """;
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = options.RoutePrefix,
                Method = "POST",
                Headers =
                {
                    ContentType = $"{MediaTypeNames.Application.Json}"
                },
                Body = new MemoryStream(Encoding.UTF8.GetBytes(json))
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        requestHandlerMock.Setup(h => h.ProcessJsonRpcRequest(It.IsAny<SingleRequestWrapper>(), httpContext, nextMock.Object))
            .ReturnsAsync((IResponseWrapper?) null)
            .Verifiable();

        await middleware.InvokeAsync(httpContext);

        nextMock.VerifyNoOtherCalls();
        requestHandlerMock.Verify();
        httpContext.Response.Body.Length.Should().Be(0);
    }

    private const int Id = 123;
    private const string MethodName = "methodName";
}
