using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Features;

namespace Tochka.JsonRpc.Server.Tests.Extensions;

[TestFixture]
internal class HttpContextExtensionsTests
{
    [Test]
    public void GetJsonRpcCall_NoFeature_ReturnNull()
    {
        var httpContext = new DefaultHttpContext();

        var result = httpContext.GetJsonRpcCall();

        result.Should().BeNull();
    }

    [Test]
    public void GetJsonRpcCall_NoCall_ReturnNull()
    {
        var httpContext = new DefaultHttpContext();
        var feature = new JsonRpcFeature();
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        var result = httpContext.GetJsonRpcCall();

        result.Should().BeNull();
    }

    [Test]
    public void GetJsonRpcCall_HasCall_ReturnCall()
    {
        var httpContext = new DefaultHttpContext();
        var call = Mock.Of<ICall>();
        var feature = new JsonRpcFeature { Call = call };
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        var result = httpContext.GetJsonRpcCall();

        result.Should().Be(call);
    }

    [Test]
    public void GetRawJsonRpcCall_NoFeature_ReturnNull()
    {
        var httpContext = new DefaultHttpContext();

        var result = httpContext.GetRawJsonRpcCall();

        result.Should().BeNull();
    }

    [Test]
    public void GetRawJsonRpcCall_NoRawCall_ReturnNull()
    {
        var httpContext = new DefaultHttpContext();
        var feature = new JsonRpcFeature();
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        var result = httpContext.GetRawJsonRpcCall();

        result.Should().BeNull();
    }

    [Test]
    public void GetRawJsonRpcCall_HasRawCall_ReturnRawCall()
    {
        var httpContext = new DefaultHttpContext();
        var rawCall = JsonDocument.Parse("{}");
        var feature = new JsonRpcFeature { RawCall = rawCall };
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        var result = httpContext.GetRawJsonRpcCall();

        result.Should().Be(rawCall);
    }

    [Test]
    public void GetJsonRpcResponse_NoFeature_ReturnNull()
    {
        var httpContext = new DefaultHttpContext();

        var result = httpContext.GetJsonRpcResponse();

        result.Should().BeNull();
    }

    [Test]
    public void GetJsonRpcResponse_NoResponse_ReturnNull()
    {
        var httpContext = new DefaultHttpContext();
        var feature = new JsonRpcFeature();
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        var result = httpContext.GetJsonRpcResponse();

        result.Should().BeNull();
    }

    [Test]
    public void GetJsonRpcResponse_HasResponseCall_ReturnResponse()
    {
        var httpContext = new DefaultHttpContext();
        var response = Mock.Of<IResponse>();
        var feature = new JsonRpcFeature { Response = response };
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        var result = httpContext.GetJsonRpcResponse();

        result.Should().Be(response);
    }

    [Test]
    public void JsonRpcRequestIsBatch_NoFeature_ReturnFalse()
    {
        var httpContext = new DefaultHttpContext();

        var result = httpContext.JsonRpcRequestIsBatch();

        result.Should().BeFalse();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void JsonRpcRequestIsBatch_HasFeature_ReturnIsBatch(bool isBatch)
    {
        var httpContext = new DefaultHttpContext();
        var feature = new JsonRpcFeature { IsBatch = isBatch };
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        var result = httpContext.JsonRpcRequestIsBatch();

        result.Should().Be(isBatch);
    }

    [Test]
    public void SetJsonRpcResponse_NoFeature_SetFeatureAndResponse()
    {
        var httpContext = new DefaultHttpContext();
        var response = Mock.Of<IResponse>();

        httpContext.SetJsonRpcResponse(response);

        var expected = new JsonRpcFeature { Response = response };
        httpContext.Features.Get<IJsonRpcFeature>().Should().BeEquivalentTo(expected);
    }

    [Test]
    public void SetJsonRpcResponse_HasFeature_SetResponse()
    {
        var httpContext = new DefaultHttpContext();
        var response = Mock.Of<IResponse>();
        var feature = new JsonRpcFeature();
        httpContext.Features.Set<IJsonRpcFeature>(feature);

        httpContext.SetJsonRpcResponse(response);

        feature.Response.Should().Be(response);
    }
}
