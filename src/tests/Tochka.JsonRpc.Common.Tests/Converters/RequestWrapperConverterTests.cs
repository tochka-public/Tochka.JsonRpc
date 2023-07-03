using System;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Common.Tests.Converters;

[TestFixture]
internal class RequestWrapperConverterTests
{
    [Test]
    public void Serialize_Throw()
    {
        IRequestWrapper wrapper = new SingleRequestWrapper(JsonDocument.Parse("{}"));

        var action = () => JsonSerializer.Serialize(wrapper, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Deserialize_Single()
    {
        var json = "{}";

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
    }

    [Test]
    public void Deserialize_Batch()
    {
        var json = "[]";

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<BatchRequestWrapper>();
    }

    // Other possible values from ECMA-404, Section 5:
    [TestCase("\"\"")]
    [TestCase("\"str\"")]
    [TestCase("0")]
    [TestCase("0.1")]
    [TestCase("true")]
    [TestCase("false")]
    public void Deserialize_UnknownType_Throw(string json)
    {
        var action = () => JsonSerializer.Deserialize<IRequestWrapper>(json, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }
}
