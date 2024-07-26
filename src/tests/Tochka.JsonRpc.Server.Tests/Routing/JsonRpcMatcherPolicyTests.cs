using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Features;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Routing;

namespace Tochka.JsonRpc.Server.Tests.Routing;

[TestFixture]
public class JsonRpcMatcherPolicyTests
{
    private JsonRpcMatcherPolicy matcherPolicy;

    [SetUp]
    public void Setup() => matcherPolicy = new JsonRpcMatcherPolicy();

    [Test]
    public void ApplyAsync_NoEndpoints_ReturnFalse()
    {
        var endpoints = Array.Empty<Endpoint>();

        var result = matcherPolicy.AppliesToEndpoints(endpoints);

        result.Should().BeFalse();
    }

    [Test]
    public void ApplyAsync_NoJsonRpcControllerAttribute_ReturnFalse()
    {
        var metadata = new object[]
        {
            new JsonRpcMethodAttribute(MethodName)
        };
        var endpoints = new List<Endpoint>
        {
            new(null, new EndpointMetadataCollection(metadata), null)
        };

        var result = matcherPolicy.AppliesToEndpoints(endpoints);

        result.Should().BeFalse();
    }

    [Test]
    public void ApplyAsync_NoJsonRpcMethodAttribute_ReturnFalse()
    {
        var metadata = new object[]
        {
            new JsonRpcControllerAttribute()
        };
        var endpoints = new List<Endpoint>
        {
            new(null, new EndpointMetadataCollection(metadata), null)
        };

        var result = matcherPolicy.AppliesToEndpoints(endpoints);

        result.Should().BeFalse();
    }

    [Test]
    public void ApplyAsync_AtLeastOneEndpointHasControllerAndMethodAttributes_ReturnTrue()
    {
        var metadata = new object[]
        {
            new JsonRpcControllerAttribute(),
            new JsonRpcMethodAttribute(MethodName)
        };
        var endpoints = new List<Endpoint>
        {
            new(null, new EndpointMetadataCollection(metadata), null),
            new(null, new EndpointMetadataCollection(), null)
        };

        var result = matcherPolicy.AppliesToEndpoints(endpoints);

        result.Should().BeTrue();
    }

    [Test]
    public async Task ApplyAsync_NoJsonRpcCall_RejectAll()
    {
        var httpContext = new DefaultHttpContext();
        var endpoints = new[]
        {
            new Endpoint(null, new EndpointMetadataCollection(), null),
            new Endpoint(null, new EndpointMetadataCollection(), null)
        };
        var values = new[]
        {
            new RouteValueDictionary(),
            new RouteValueDictionary()
        };
        var scores = new[]
        {
            0,
            0
        };
        var candidates = new CandidateSet(endpoints, values, scores);

        await matcherPolicy.ApplyAsync(httpContext, candidates);

        for (var i = 0; i < candidates.Count; i++)
        {
            candidates.IsValidCandidate(i).Should().BeFalse();
        }
    }

    [Test]
    public async Task ApplyAsync_HasJsonRpcCall_SetValidityBasedOnMethod()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = new UntypedNotification(MethodName, null) });
        var endpoints = new[]
        {
            new Endpoint(null, new EndpointMetadataCollection(new JsonRpcMethodAttribute(MethodName)), null),
            new Endpoint(null, new EndpointMetadataCollection(new JsonRpcMethodAttribute("otherMethod")), null)
        };
        var values = new[]
        {
            new RouteValueDictionary(),
            new RouteValueDictionary()
        };
        var scores = new[]
        {
            0,
            0
        };
        var candidates = new CandidateSet(endpoints, values, scores);

        await matcherPolicy.ApplyAsync(httpContext, candidates);

        candidates.IsValidCandidate(0).Should().BeTrue();
        candidates.IsValidCandidate(1).Should().BeFalse();
    }

    [Test]
    public async Task ApplyAsync_NoValidCandidates_ThrowJsonRpcMethodNotFoundException()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = new UntypedNotification(MethodName, null) });
        var endpoints = new[]
        {
            new Endpoint(null, new EndpointMetadataCollection(new JsonRpcMethodAttribute("otherMethod")), null),
            new Endpoint(null, new EndpointMetadataCollection(new JsonRpcMethodAttribute("andAnotherOne")), null)
        };
        var values = new[]
        {
            new RouteValueDictionary(),
            new RouteValueDictionary()
        };
        var scores = new[]
        {
            0,
            0
        };
        var candidates = new CandidateSet(endpoints, values, scores);

        var action = () => matcherPolicy.ApplyAsync(httpContext, candidates);

        await action.Should().ThrowAsync<JsonRpcMethodNotFoundException>();
    }

    private const string MethodName = "methodName";
}
