using System;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace Tochka.JsonRpc.OpenRpc.Tests.Converters;

[TestFixture]
internal class UriConverterTests
{
    [Test]
    public void Deserialize_Throw()
    {
        var json = "\"https://for_tests.localhost\"";

        var action = () => JsonSerializer.Deserialize<Uri>(json, OpenRpcConstants.JsonSerializerOptions);

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Serialize_SimpleUri_SerializeSuccessfully()
    {
        var uriStr = "https://for_tests.localhost/";
        var uri = new Uri(uriStr);

        var serialized = JsonSerializer.Serialize(uri, OpenRpcConstants.JsonSerializerOptions);

        serialized.Should().Be($"\"{uriStr}\"");
    }

    [Test]
    public void Serialize_TemplateUri_DontEscapeBrackets()
    {
        var uriStr = "https://for_tests.localhost/[controller]/{arg}/";
        var uri = new Uri(uriStr);

        var serialized = JsonSerializer.Serialize(uri, OpenRpcConstants.JsonSerializerOptions);

        serialized.Should().Be($"\"{uriStr}\"");
    }
}
