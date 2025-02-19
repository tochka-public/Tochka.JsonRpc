using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace Tochka.JsonRpc.Common.Tests;

[TestFixture]
public class UtilsTests
{
    [Test]
    public void DeserializeErrorData_DataIsNull_ReturnDefault()
    {
        var deserialized = Utils.DeserializeErrorData<object>(null, new JsonSerializerOptions(), new JsonSerializerOptions());

        deserialized.Should().BeNull();
    }

    [Test]
    public void DeserializeErrorData_DataJsonSerializerOptionsFailedWithNotSupportedException_UseHeadersJsonSerializerOptions()
    {
        var dataJsonSerializerOptions = new JsonSerializerOptions();
        var headersJsonSerializerOptions = new JsonSerializerOptions { Converters = { new IErrorDataConverter() } };
        var data = JsonDocument.Parse("{\"A\": \"b\"}");

        var deserialized = Utils.DeserializeErrorData<IErrorData>(data, headersJsonSerializerOptions, dataJsonSerializerOptions);

        deserialized.Should().BeEquivalentTo(new ErrorData("b"));
    }

    [Test]
    public void DeserializeErrorData_DataJsonSerializerOptionsFailedWithJsonException_UseHeadersJsonSerializerOptions()
    {
        var dataJsonSerializerOptions = new JsonSerializerOptions();
        var headersJsonSerializerOptions = new JsonSerializerOptions { Converters = { new ErrorDataConverter() } };
        var data = JsonDocument.Parse("\"a\"");

        var deserialized = Utils.DeserializeErrorData<ErrorData>(data, headersJsonSerializerOptions, dataJsonSerializerOptions);

        deserialized.Should().BeEquivalentTo(new ErrorData("a"));
    }

    [Test]
    public void SerializeParams_DataIsNull_ReturnNull()
    {
        var serialized = Utils.SerializeParams<object>(null, new JsonSerializerOptions());

        serialized.Should().BeNull();
    }

    [Test]
    public void SerializeParams_DataIsIEnumerable_DontThrow()
    {
        IEnumerable<string> data = Array.Empty<string>();

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions());

        action.Should().NotThrow();
    }

    [Test]
    public void SerializeParams_DataIsICollection_DontThrow()
    {
        ICollection<string> data = Array.Empty<string>();

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions());

        action.Should().NotThrow();
    }

    [Test]
    public void SerializeParams_DataIsArray_DontThrow()
    {
        var data = Array.Empty<string>();

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions());

        action.Should().NotThrow();
    }

    [Test]
    public void SerializeParams_DataIsList_DontThrow()
    {
        var data = new List<string>();

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions());

        action.Should().NotThrow();
    }

    [Test]
    public void SerializeParams_DataIsObject_DontThrow()
    {
        var data = new { a = "b" };

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions());

        action.Should().NotThrow();
    }

    [Test]
    public void SerializeParams_DataIsEmptyObject_DontThrow()
    {
        var data = new { };

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions());

        action.Should().NotThrow();
    }

    [Test]
    public void SerializeParams_DataIsString_Throw()
    {
        var data = "str";

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions());

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void SerializeParams_DataSerializedNotAsObjectOrArray_Throw()
    {
        var data = new ErrorData("a");

        var action = () => Utils.SerializeParams(data, new JsonSerializerOptions { Converters = { new ErrorDataConverter() } });

        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void GetPropertyNames_ReturnsOnlyTopLevelProperties()
    {
        const string property1 = "prop1";
        const string property2 = "prop2";
        var json =
            $$"""
              {
                  "{{property1}}": "value1",
                  "{{property2}}": {
                      "prop3": "value3"
                  }
              }
              """;
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();

        var properties = Utils.GetPropertyNames(ref reader);

        properties.Should().BeEquivalentTo(property1, property2);
    }

    [Test]
    public void GetPropertyNames_ReturnsOnlyCurrentObjectProperties()
    {
        const string property1 = "prop1";
        const string property2 = "prop2";
        var json =
            $$"""
              [
                  {
                      "{{property1}}": "value1",
                      "{{property2}}": "value2"
                  },
                  {
                      "prop3": "value3"
                  }
              ]
              """;
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // to start reading properties from '{'
        reader.Read();

        var properties = Utils.GetPropertyNames(ref reader);

        properties.Should().BeEquivalentTo(property1, property2);
    }

    private interface IErrorData
    {
    }

    private sealed class ErrorDataConverter : JsonConverter<ErrorData>
    {
        public override ErrorData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetString());

        public override void Write(Utf8JsonWriter writer, ErrorData value, JsonSerializerOptions options) =>
            writer.WriteRawValue("42");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Just for tests")]
    private sealed class IErrorDataConverter : JsonConverter<IErrorData>
    {
        public override IErrorData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<ErrorData>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, IErrorData value, JsonSerializerOptions options) =>
            throw new NotImplementedException();
    }

    private sealed record ErrorData
    (
        string A
    ) : IErrorData;
}
