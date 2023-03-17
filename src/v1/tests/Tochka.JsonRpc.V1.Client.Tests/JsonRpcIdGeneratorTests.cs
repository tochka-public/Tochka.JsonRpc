using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Client.Services;

namespace Tochka.JsonRpc.V1.Client.Tests
{
    public class JsonRpcIdGeneratorTests
    {
        private JsonRpcIdGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new JsonRpcIdGenerator(Mock.Of<ILogger<JsonRpcIdGenerator>>());
        }

        [Test]
        public void Test_GenerateId_ReturnsDifferentValues()
        {
            var x = generator.GenerateId();
            var y = generator.GenerateId();
            x.Should().NotBe(y);
        }
    }
}