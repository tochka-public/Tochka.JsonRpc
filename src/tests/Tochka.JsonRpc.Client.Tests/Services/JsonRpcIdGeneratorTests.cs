using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Services;

namespace Tochka.JsonRpc.Client.Tests.Services;

[TestFixture]
public class JsonRpcIdGeneratorTests
{
    private JsonRpcIdGenerator generator;

    [SetUp]
    public void Setup() => generator = new JsonRpcIdGenerator();

    [Test]
    public void GenerateId_ReturnsDifferentIds()
    {
        var x = generator.GenerateId();
        var y = generator.GenerateId();
        x.Should().NotBeEquivalentTo(y, static options => options.RespectingRuntimeTypes());
    }
}
