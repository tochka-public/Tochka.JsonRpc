using System.Net.Mime;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Extensions;

namespace Tochka.JsonRpc.Server.Tests.Services;

[TestFixture]
public class JsonRpcRequestAcceptHeaderTests
{

    [TestCase(JsonRpcConstants.ContentType, JsonRpcConstants.ContentType)]
    [TestCase(JsonRpcConstants.ContentType, JsonRpcConstants.ContentType, "application/jsonrequest", "application/json-rpc", MediaTypeNames.Application.Xml)]
    [TestCase(JsonRpcConstants.ContentType, "application/json-rpc", JsonRpcConstants.ContentType, "application/jsonrequest", MediaTypeNames.Application.Xml)]
    [TestCase("application/json-rpc", "application/json-rpc", MediaTypeNames.Application.Xml)]
    [TestCase("application/json-rpc", MediaTypeNames.Application.Xml, "application/json-rpc")]
    [TestCase(JsonRpcConstants.ContentType)]
    [TestCase(JsonRpcConstants.ContentType, "*/*")]
    [TestCase(JsonRpcConstants.ContentType, "*/*", MediaTypeNames.Application.Xml)]
    [TestCase(JsonRpcConstants.ContentType, "*/*", "application/json-rpc")]

    [TestCase(null, MediaTypeNames.Application.Xml)]
    [TestCase(null, MediaTypeNames.Text.Plain, MediaTypeNames.Application.Xml)]
    public void AcceptHeader_Parse_Expected(string? expected, params string[] acceptHeaders)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Headers =
                {
                    Accept = acceptHeaders
                }
            }
        };
        var responseMediaType = httpContext.GetJsonRpcResponseMediaType();
        responseMediaType.Should().Be(expected);
    }

}
