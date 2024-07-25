using System;
using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Tests.Converters;

[TestFixture]
public class JsonRpcIdConverterTests
{
    [Test]
    public void Serialize_Number()
    {
        IRpcId id = new NumberRpcId(42);

        var serialized = JsonSerializer.Serialize(id, JsonRpcSerializerOptions.Headers);

        serialized.Should().Be("42");
    }

    [Test]
    public void Serialize_FloatNumber()
    {
        IRpcId id = new FloatNumberRpcId(42.5F);

        var serialized = JsonSerializer.Serialize(id, JsonRpcSerializerOptions.Headers);

        serialized.Should().Be("42.5");
    }

    [Test]
    public void Serialize_String()
    {
        IRpcId id = new StringRpcId("test");

        var serialized = JsonSerializer.Serialize(id, JsonRpcSerializerOptions.Headers);

        serialized.Should().Be("\"test\"");
    }

    [Test]
    public void Serialize_StringWithNumber()
    {
        IRpcId id = new StringRpcId("123");

        var serialized = JsonSerializer.Serialize(id, JsonRpcSerializerOptions.Headers);

        serialized.Should().Be("\"123\"");
    }

    [Test]
    public void Serialize_Null()
    {
        IRpcId id = new NullRpcId();

        var serialized = JsonSerializer.Serialize(id, JsonRpcSerializerOptions.Headers);

        serialized.Should().Be("null");
    }

    [Test]
    public void Serialize_UnknownType_Throw()
    {
        var id = Mock.Of<IRpcId>();

        var action = () => JsonSerializer.Serialize(id, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase("")]
    [TestCase("test")]
    [TestCase("123")]
    public void Deserialize_String(string value)
    {
        var json = $"\"{value}\"";

        var deserialized = JsonSerializer.Deserialize<IRpcId>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<StringRpcId>().Subject.Value.Should().Be(value);
    }

    [TestCase(0)]
    [TestCase(42)]
    [TestCase(-1)]
    [TestCase(int.MaxValue)]
    public void Deserialize_IntNumber(int value)
    {
        var json = JsonSerializer.Serialize(value);

        var deserialized = JsonSerializer.Deserialize<IRpcId>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<NumberRpcId>().Subject.Value.Should().Be(value);
    }

    [TestCase(0L)]
    [TestCase(42L)]
    [TestCase(-1L)]
    [TestCase(long.MaxValue)]
    public void Deserialize_LongNumber(long value)
    {
        var json = JsonSerializer.Serialize(value);

        var deserialized = JsonSerializer.Deserialize<IRpcId>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<NumberRpcId>().Subject.Value.Should().Be(value);
    }

    [TestCase(0.5F)]
    [TestCase(42.735F)]
    [TestCase(-1.1F)]
    [TestCase(float.MaxValue)]
    public void Deserialize_FloatNumber(double value)
    {
        var json = JsonSerializer.Serialize(value);

        var deserialized = JsonSerializer.Deserialize<IRpcId>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<FloatNumberRpcId>().Subject.Value.Should().Be(value);
    }

    [TestCase(0.5)]
    [TestCase(42.735)]
    [TestCase(-1.1)]
    [TestCase(double.MaxValue)]
    public void Deserialize_DoubleNumber(double value)
    {
        var json = JsonSerializer.Serialize(value);

        var deserialized = JsonSerializer.Deserialize<IRpcId>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<FloatNumberRpcId>().Subject.Value.Should().Be(value);
    }

    [TestCase("0.5")]
    [TestCase("42.735")]
    [TestCase("-1.1")]
    public void SerializeDeserialize_RpcIdTheSame(string idJson)
    {
        var deserialized = JsonSerializer.Deserialize<IRpcId>(idJson, JsonRpcSerializerOptions.Headers);
        var serialized = JsonSerializer.Serialize<IRpcId>(deserialized, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<FloatNumberRpcId>().Subject.Value.ToString(CultureInfo.InvariantCulture)
            .Should().Be(serialized);
    }

    [Test]
    public void Deserialize_NullForNull()
    {
        var json = "null";

        var deserialized = JsonSerializer.Deserialize<IRpcId>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<NullRpcId>();
    }

    // Other possible values from ECMA-404, Section 5:
    [TestCase("{}")]
    [TestCase("[]")]
    [TestCase("true")]
    [TestCase("false")]
    public void Deserialize_BadId_Throws(string json)
    {
        var action = () => JsonSerializer.Deserialize<IRpcId>(json, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }
}
