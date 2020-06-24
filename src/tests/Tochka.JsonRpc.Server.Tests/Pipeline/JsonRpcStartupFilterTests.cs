using System;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Pipeline
{
    public class JsonRpcStartupFilterTests
    {
        private TestEnvironment testEnvironment;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services => { services.AddSingleton<JsonRpcStartupFilter>(); });
        }

        [Test]
        public void Test_Configure_AddsMiddleware()
        {
            var filter = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcStartupFilter>();
            var builderMock = new Mock<IApplicationBuilder>();

            var action = filter.Configure(x => { });
            action(builderMock.Object);

            builderMock.Verify(x => x.Use(It.Is<Func<RequestDelegate, RequestDelegate>>(y => CheckMiddlewareType(y))), Times.Once);
        }

        /// <summary>
        /// Check that our middleware is registered. This relise on framework internals, so might break in 3.x
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private bool CheckMiddlewareType(Func<RequestDelegate, RequestDelegate> func)
        {
            var middlewareType = func.Target.GetType().GetField("middleware").GetValue(func.Target) as Type;
            middlewareType.Should().Be<JsonRpcMiddleware>();
            return true;
        }
    }
}