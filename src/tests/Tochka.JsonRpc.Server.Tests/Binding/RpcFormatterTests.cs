using System.Buffers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Binding
{
    public class RpcFormatterTests
    {
        private TestEnvironment testEnvironment;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
                services.AddSingleton<HeaderRpcSerializer>();
                services.AddSingleton(Mock.Of<ArrayPool<char>>());
                services.AddSingleton<RpcFormatter, RpcFormatterPublic>();
            });
        }

        [Test]
        public void Test_CreateJsonSerializer_ReturnsHeaderSerializer()
        {
            var serializer = testEnvironment.ServiceProvider.GetRequiredService<HeaderRpcSerializer>();
            var formatter = testEnvironment.ServiceProvider.GetRequiredService<RpcFormatter>() as RpcFormatterPublic;

            formatter.CreateJsonSerializerPublic().Should().Be(serializer.Serializer);
        }
    }
}