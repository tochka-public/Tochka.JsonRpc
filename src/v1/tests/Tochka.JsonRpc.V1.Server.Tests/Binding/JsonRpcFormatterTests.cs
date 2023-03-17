using System.Buffers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server.Binding;
using Tochka.JsonRpc.V1.Server.Tests.Helpers;

namespace Tochka.JsonRpc.V1.Server.Tests.Binding
{
    public class JsonRpcFormatterTests
    {
        private TestEnvironment testEnvironment;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
                services.AddSingleton<HeaderJsonRpcSerializer>();
                services.AddSingleton(Mock.Of<ArrayPool<char>>());
                services.AddSingleton<JsonRpcFormatter, JsonRpcFormatterPublic>();
            });
        }

        [Test]
        public void Test_CreateJsonSerializer_ReturnsHeaderSerializer()
        {
            var serializer = testEnvironment.ServiceProvider.GetRequiredService<HeaderJsonRpcSerializer>();
            var formatter = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcFormatter>() as JsonRpcFormatterPublic;

            formatter.CreateJsonSerializerPublic().Should().Be(serializer.Serializer);
        }
    }
}
