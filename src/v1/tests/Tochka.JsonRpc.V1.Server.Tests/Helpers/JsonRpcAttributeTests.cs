using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Server.Attributes;
using Tochka.JsonRpc.V1.Server.Models;
using Tochka.JsonRpc.V1.Server.Services;
using Tochka.JsonRpc.V1.Server.Settings;

namespace Tochka.JsonRpc.V1.Server.Tests.Helpers
{
    public class JsonRpcAttributeTests
    {
        private TestEnvironment testEnvironment;
        private Mock<IMethodMatcher> matcherMock;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
                matcherMock = new Mock<IMethodMatcher>();
                services.AddSingleton(matcherMock.Object);
            });
        }

        [Test]
        public void Test_IsValidForRequest_FalseWhenNoItem()
        {
            var items = new Dictionary<object, object>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(items);
            var attribute = new JsonRpcAttribute();
            var result = attribute.IsValidForRequest(new RouteContext(httpContextMock.Object), Mock.Of<ActionDescriptor>());

            result.Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test_IsValidForRequest_ReturnsMatcherResult(bool expected)
        {
            matcherMock.Setup(x => x.IsMatch(It.IsAny<MethodMetadata>(), It.IsAny<string>()))
                .Returns(expected);
            var items = new Dictionary<object, object>()
            {
                [JsonRpcConstants.NestedPipelineItemKey] = true,
                [JsonRpcConstants.RequestItemKey] = Mock.Of<IUntypedCall>(),
            };
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Items)
                .Returns(items);
            httpContextMock.SetupGet(x => x.RequestServices)
                .Returns(testEnvironment.ServiceProvider);
            var properties = new Dictionary<object, object>
            {
                [typeof(MethodMetadata)] = new MethodMetadata(new JsonRpcMethodOptions(), new JsonName("test", "test"), new JsonName("test", "test"))
            };
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Object.Properties = properties;
            var attribute = new JsonRpcAttribute();

            var result = attribute.IsValidForRequest(new RouteContext(httpContextMock.Object), actionDescriptorMock.Object);

            result.Should().Be(expected);
            matcherMock.Verify(x => x.IsMatch(It.IsAny<MethodMetadata>(), It.IsAny<string>()));
            matcherMock.VerifyNoOtherCalls();
        }
    }
}
