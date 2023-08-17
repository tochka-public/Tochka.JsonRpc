using System;
using System.Net.Mime;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Services;

[TestFixture]
internal class JsonRpcRequestValidatorTests
{
    private JsonRpcServerOptions options;
    private JsonRpcRequestValidator validator;

    [SetUp]
    public void Setup()
    {
        options = new JsonRpcServerOptions();

        validator = new JsonRpcRequestValidator(Options.Create(options));
    }

    [TestCase(null)]
    [TestCase("")]
    public void Ctor_RoutePrefixEmpty_Throw(string? routePrefix)
    {
        options.RoutePrefix = routePrefix;

        var action = () => new JsonRpcRequestValidator(Options.Create(options));

        action.Should().Throw<ArgumentNullException>();
    }

    [TestCase("/{*all}")]
    [TestCase("/{*all}/")]
    [TestCase("/smth/{*all}")]
    [TestCase("/smth/{*all}/")]
    [TestCase("/smth/{arg}/{*all}")]
    [TestCase("/smth/{arg}/{*all}/")]
    [TestCase("/{arg}/smth/{*all}")]
    [TestCase("/{arg}/smth/{*all}/")]
    public void RouteWithWildcardSuffixRegex_RoutePrefixEndsWithWildcard_Match(string routePrefix)
    {
        var result = JsonRpcRequestValidator.RouteWithWildcardSuffixRegex.IsMatch(routePrefix);

        result.Should().BeTrue();
    }

    [TestCase("/")]
    [TestCase("/smth")]
    [TestCase("/smth/")]
    [TestCase("/smth/{arg}")]
    [TestCase("/smth/{arg}/")]
    [TestCase("/{arg}/smth")]
    [TestCase("/{arg}/smth/")]
    public void RouteWithWildcardSuffixRegex_RoutePrefixDoesntEndWithWildcard_DontMatch(string routePrefix)
    {
        var result = JsonRpcRequestValidator.RouteWithWildcardSuffixRegex.IsMatch(routePrefix);

        result.Should().BeFalse();
    }

    [TestCase("/{*all}")]
    [TestCase("/{*all}/")]
    [TestCase("/smth/{*all}")]
    [TestCase("/smth/{*all}/")]
    [TestCase("/smth/{arg}/{*all}")]
    [TestCase("/smth/{arg}/{*all}/")]
    [TestCase("/{arg}/smth/{*all}")]
    [TestCase("/{arg}/smth/{*all}/")]
    public void BuildRouteTemplate_RoutePrefixEndsWithWildcard_DontAddWildcardSuffix(string routePrefix)
    {
        var result = JsonRpcRequestValidator.BuildRouteTemplate(routePrefix);

        result.Should().Be(routePrefix);
    }

    [TestCase("/", "/{*suffix}")]
    [TestCase("/smth", "/smth/{*suffix}")]
    [TestCase("/smth/", "/smth/{*suffix}")]
    [TestCase("/smth/{arg}", "/smth/{arg}/{*suffix}")]
    [TestCase("/smth/{arg}/", "/smth/{arg}/{*suffix}")]
    [TestCase("/{arg}/smth", "/{arg}/smth/{*suffix}")]
    [TestCase("/{arg}/smth/", "/{arg}/smth/{*suffix}")]
    public void BuildRouteTemplate_RoutePrefixDoesntEndWithWildcard_AddWildcardSuffix(string routePrefix, string expected)
    {
        var result = JsonRpcRequestValidator.BuildRouteTemplate(routePrefix);

        result.Should().Be(expected);
    }

    [TestCase("/[controller]", "/{controller}/{*suffix}")]
    [TestCase("/[action]", "/{action}/{*suffix}")]
    [TestCase("/[area]", "/{area}/{*suffix}")]
    [TestCase("/[controller]/", "/{controller}/{*suffix}")]
    [TestCase("/[action]/", "/{action}/{*suffix}")]
    [TestCase("/[area]/", "/{area}/{*suffix}")]
    [TestCase("/smth/[controller]", "/smth/{controller}/{*suffix}")]
    [TestCase("/smth/[action]", "/smth/{action}/{*suffix}")]
    [TestCase("/smth/[area]", "/smth/{area}/{*suffix}")]
    [TestCase("/smth/[controller]/", "/smth/{controller}/{*suffix}")]
    [TestCase("/smth/[action]/", "/smth/{action}/{*suffix}")]
    [TestCase("/smth/[area]/", "/smth/{area}/{*suffix}")]
    [TestCase("/[controller]/smth", "/{controller}/smth/{*suffix}")]
    [TestCase("/[action]/smth", "/{action}/smth/{*suffix}")]
    [TestCase("/[area]/smth", "/{area}/smth/{*suffix}")]
    [TestCase("/smth[controller]", "/smth{controller}/{*suffix}")]
    [TestCase("/smth[action]", "/smth{action}/{*suffix}")]
    [TestCase("/smth[area]", "/smth{area}/{*suffix}")]
    [TestCase("/smth[controller]/", "/smth{controller}/{*suffix}")]
    [TestCase("/smth[action]/", "/smth{action}/{*suffix}")]
    [TestCase("/smth[area]/", "/smth{area}/{*suffix}")]
    [TestCase("/[controller]smth", "/{controller}smth/{*suffix}")]
    [TestCase("/[action]smth", "/{action}smth/{*suffix}")]
    [TestCase("/[area]smth", "/{area}smth/{*suffix}")]
    [TestCase("/[controller]smth/", "/{controller}smth/{*suffix}")]
    [TestCase("/[action]smth/", "/{action}smth/{*suffix}")]
    [TestCase("/[area]smth/", "/{area}smth/{*suffix}")]
    [TestCase("/smth[controller]smth/", "/smth{controller}smth/{*suffix}")]
    [TestCase("/smth[action]smth/", "/smth{action}smth/{*suffix}")]
    [TestCase("/smth[area]smth/", "/smth{area}smth/{*suffix}")]
    public void BuildRouteTemplate_RoutePrefixContainsSquareBracketsParams_ReplaceWithCurlyBrackets(string routePrefix, string expected)
    {
        var result = JsonRpcRequestValidator.BuildRouteTemplate(routePrefix);

        result.Should().Be(expected);
    }

    [TestCase("/some/path")]
    [TestCase("/smth/{arg}")]
    [TestCase("/[controller]/[action]/[area]")]
    [TestCase("/smth/{*all}")]
    public void IsJsonRpcRequest_PathDoesntStartWithPrefix_ReturnFalse(string path)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = path
            }
        };

        var result = validator.IsJsonRpcRequest(httpContext);

        result.Should().BeFalse();
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
                Path = options.RoutePrefix,
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
                Path = options.RoutePrefix,
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
                Path = options.RoutePrefix,
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
                Path = $"{options.RoutePrefix}{pathSuffix}",
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
