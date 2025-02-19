using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Tochka.JsonRpc.Common.Tests;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public void Get_SeveralOccurrences_ReturnFirst()
    {
        var first = new TestClass(1);
        var second = new TestClass(2);
        var collection = new object[] { "123", first, second };

        var result = collection.Get<TestClass>();

        result.Should().Be(first);
    }

    [Test]
    public void Get_NoOccurrences_ReturnNull()
    {
        var collection = new object[] { "123", 1, true };

        var result = collection.Get<TestClass>();

        result.Should().BeNull();
    }

    [Test]
    public void ConvertName_PolicyNotNull_ReturnConvertedName()
    {
        var convertedName = "converted";
        var policyMock = new Mock<JsonNamingPolicy>();
        policyMock.Setup(p => p.ConvertName(It.IsAny<string>()))
            .Returns(convertedName)
            .Verifiable();
        var options = new JsonSerializerOptions { PropertyNamingPolicy = policyMock.Object };

        var result = options.ConvertName("name");

        result.Should().Be(convertedName);
        policyMock.Verify();
    }

    [Test]
    public void ConvertName_PolicyNull_ReturnInitialName()
    {
        var name = "name";
        var options = new JsonSerializerOptions();

        var result = options.ConvertName(name);

        result.Should().Be(name);
    }

    private sealed record TestClass
    (
        int A
    );
}
