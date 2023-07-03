using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.OpenRpc.Services;

namespace Tochka.JsonRpc.OpenRpc.Tests;

[TestFixture]
internal class OpenRpcMiddlewareTests
{
    private Mock<RequestDelegate> requestDelegateMock;
    private OpenRpcOptions options;
    private Mock<IOpenRpcDocumentGenerator> documentGeneratorMock;
    private OpenRpcMiddleware middleware;

    [SetUp]
    public void Setup()
    {
        requestDelegateMock = new Mock<RequestDelegate>();
        options = new OpenRpcOptions();
        documentGeneratorMock = new Mock<IOpenRpcDocumentGenerator>();

        middleware = new OpenRpcMiddleware(requestDelegateMock.Object, Options.Create(options));
    }

    [TestCase("POST")]
    [TestCase("PATCH")]
    [TestCase("PUT")]
    [TestCase("DELETE")]
    [TestCase("HEAD")]
    [TestCase("OPTIONS")]
    [TestCase("something")]
    public async Task InvokeAsync_HttpMethodNotGet_DontDoAnything(string httpMethod)
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Method = httpMethod },
            Response = { Body = new MemoryStream() }
        };

        await middleware.InvokeAsync(httpContext, documentGeneratorMock.Object);

        httpContext.Response.Body.Should().HaveLength(0);
        httpContext.Response.ContentType.Should().BeNullOrEmpty();
        requestDelegateMock.Verify(d => d(httpContext));
        documentGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task InvokeAsync_RouteDoesntMatchTemplate_DontDoAnything()
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = HttpMethods.Get,
                Path = "/some/other/path"
            },
            Response = { Body = new MemoryStream() }
        };

        await middleware.InvokeAsync(httpContext, documentGeneratorMock.Object);

        httpContext.Response.Body.Should().HaveLength(0);
        httpContext.Response.ContentType.Should().BeNullOrEmpty();
        requestDelegateMock.Verify(d => d(httpContext));
        documentGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task InvokeAsync_RouteTemplateDoesntHaveDocNameParameter_DontDoAnything()
    {
        var path = "some/other/path";
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = HttpMethods.Get,
                Path = $"/{path}"
            },
            Response = { Body = new MemoryStream() }
        };
        options.DocumentPath = path;

        await middleware.InvokeAsync(httpContext, documentGeneratorMock.Object);

        httpContext.Response.Body.Should().HaveLength(0);
        httpContext.Response.ContentType.Should().BeNullOrEmpty();
        requestDelegateMock.Verify(d => d(httpContext));
        documentGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task InvokeAsync_OptionsDontHaveDocByName_Return404()
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = HttpMethods.Get,
                Path = "/openrpc/jsonrpc.json"
            },
            Response = { Body = new MemoryStream() }
        };

        await middleware.InvokeAsync(httpContext, documentGeneratorMock.Object);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        httpContext.Response.Body.Should().HaveLength(0);
        httpContext.Response.ContentType.Should().BeNullOrEmpty();
        requestDelegateMock.Verify(d => d(httpContext), Times.Never);
        documentGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task InvokeAsync_OptionsHaveDocByName_GenerateDocument()
    {
        var documentName = "jsonrpc";
        var scheme = "https";
        var host = "localhost";
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = HttpMethods.Get,
                Path = $"/openrpc/{documentName}.json",
                Scheme = scheme,
                Host = new HostString(host)
            },
            Response = { Body = new MemoryStream() }
        };
        var info = new OpenRpcInfo("title", "version");
        var document = new Models.OpenRpc(info);
        options.Docs[documentName] = info;
        documentGeneratorMock.Setup(g => g.Generate(info, documentName, new Uri($"{scheme}://{host}")))
            .Returns(document)
            .Verifiable();

        await middleware.InvokeAsync(httpContext, documentGeneratorMock.Object);

        var expected = JsonSerializer.Serialize(document, OpenRpcConstants.JsonSerializerOptions);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.Body.Should().HaveLength(expected.Length);
        httpContext.Response.ContentType.Should().Be("application/json;charset=utf-8");
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        var result = await reader.ReadToEndAsync();
        result.Should().Be(expected);
        requestDelegateMock.Verify(d => d(httpContext), Times.Never);
        documentGeneratorMock.Verify();
    }
}
