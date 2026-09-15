using System;
using System.Net.Mime;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Services;

[TestFixture]
public class JsonRpcRequestValidatorTests
{
    private JsonRpcRequestValidator validator;

    [SetUp]
    public void Setup()
    {
        var options = Options.Create(new JsonRpcServerOptions(){UrlMarkerSegment = "test"});
        validator = new JsonRpcRequestValidator(options);
    }

    [TestCase("GET")]
    [TestCase("PUT")]
    [TestCase("PATCH")]
    [TestCase("DELETE")]
    [TestCase("HEAD")]
    [TestCase("OPTIONS")]
    [TestCase("something")]
    public void IsJsonRpcRequest_MethodNotPost_ReturnFalse(string method)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = "/test",
                Method = method
            }
        };

        var result = validator.IsJsonRpcRequest(httpContext);

        result.Should().BeFalse();
    }

    [Test]
    public void IsJsonRpcRequest_ContentTypeIsNull_ReturnFalse()
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = "/test",
                Method = "POST",
                Headers =
                {
                    ContentType = StringValues.Empty // it will set typed header to null
                }
            }
        };

        var result = validator.IsJsonRpcRequest(httpContext);

        result.Should().BeFalse();
    }

    [TestCase(MediaTypeNames.Text.Plain)]
    [TestCase(MediaTypeNames.Text.Html)]
    [TestCase(MediaTypeNames.Application.Xml)]
    [TestCase(MediaTypeNames.Application.Octet)]
    [TestCase(MediaTypeNames.Image.Jpeg)]
    public void IsJsonRpcRequest_MediaTypeNotJson_ReturnFalse(string contentType)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = "/test",
                Method = "POST",
                Headers =
                {
                    ContentType = contentType
                }
            }
        };

        var result = validator.IsJsonRpcRequest(httpContext);

        result.Should().BeFalse();
    }

    [TestCaseSource(typeof(JsonRpcConstants), nameof(JsonRpcConstants.AllowedRequestContentType))]
    public void IsJsonRpcRequest_MediaTypeNotJson_ReturnTrue(string contentType)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = "/test",
                Method = "POST",
                Headers =
                {
                    ContentType = contentType
                }
            }
        };

        var result = validator.IsJsonRpcRequest(httpContext);

        result.Should().BeTrue();
    }



    [TestCase("")]
    [TestCase("/")]
    [TestCase("/smth")]
    [TestCase("/smth/")]
    [TestCase("/{arg}")]
    [TestCase("/{arg}/")]
    [TestCase("/{*all}")]
    [TestCase("/{*all}/")]
    [TestCase("/[controller]/[action]/[area]")]
    [TestCase("/[controller]/[action]/[area]/")]
    public void IsJsonRpcRequest_ValidRequest_ReturnTrue(string pathSuffix)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = $"/test{pathSuffix}",
                Method = "POST",
                Headers =
                {
                    ContentType = MediaTypeNames.Application.Json
                }
            }
        };

        var result = validator.IsJsonRpcRequest(httpContext);

        result.Should().BeTrue();
    }
}
