using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Server.Models;
using Tochka.JsonRpc.V1.Server.Services;
using Tochka.JsonRpc.V1.Server.Settings;
using Tochka.JsonRpc.V1.Server.Tests.Helpers;

namespace Tochka.JsonRpc.V1.Server.Tests.Services
{
    public class MethodMatcherTests
    {
        private TestEnvironment testEnvironment;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
                services.AddSingleton<MethodMatcher>();
            });
        }

        [TestCase(MethodStyle.ControllerAndAction, "controller.action")]
        [TestCase(MethodStyle.ActionOnly, "action")]
        public void Test_GetActionName_ReturnsName(MethodStyle style, string expected)
        {
            var matcher = testEnvironment.ServiceProvider.GetRequiredService<MethodMatcher>();
            var metadata = new MethodMetadata(new JsonRpcMethodOptions()
            {
                MethodStyle = style
            }, new JsonName("", "controller"), new JsonName("", "action"));
            var result = matcher.GetActionName(metadata);
            result.Should().Be(expected);
        }

        [Test]
        public void Test_GetActionName_ThrowsOnUnknown()
        {
            var matcher = testEnvironment.ServiceProvider.GetRequiredService<MethodMatcher>();
            var metadata = new MethodMetadata(new JsonRpcMethodOptions()
            {
                MethodStyle = (MethodStyle)(-1)
            }, new JsonName("", ""), new JsonName("", ""));
            Action action = () => matcher.GetActionName(metadata);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestCase("ACTION", true)]
        [TestCase("action", true)]
        [TestCase("Action", true)]
        [TestCase("123", false)]
        [TestCase("", false)]
        public void Test_IsMatch_ComparesIgnoreCase(string method, bool expected)
        {
            var matcher = testEnvironment.ServiceProvider.GetRequiredService<MethodMatcher>();
            var metadata = new MethodMetadata(new JsonRpcMethodOptions()
            {
                MethodStyle = MethodStyle.ActionOnly
            }, new JsonName("", ""), new JsonName("", "action"));

            var result = matcher.IsMatch(metadata, method);

            result.Should().Be(expected);
        }
    }
}