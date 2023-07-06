using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Json.More;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Binding;

[TestFixture]
internal class JsonRpcParamsParserTests
{
    private JsonRpcParamsParser paramsParser;

    [SetUp]
    public void Setup() => paramsParser = new JsonRpcParamsParser(Mock.Of<ILogger<JsonRpcParamsParser>>());

    [Test]
    public void Parse_JsonObjectToDefaultPropertyWithNullValue_ReturnNullParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse($$"""{ "{{PropertyName}}": null }""");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new NullParseResult($"{JsonRpcConstants.ParamsProperty}.{PropertyName}");
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_JsonObjectToDefaultPropertyWithValue_ReturnSuccessParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var value = 123;
        var parameters = JsonDocument.Parse($$"""{ "{{PropertyName}}": {{value}} }""");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new SuccessParseResult(JsonDocument.Parse($"{value}").RootElement, $"{JsonRpcConstants.ParamsProperty}.{PropertyName}");
        result.Should().BeEquivalentTo(expected, JsonEquivalencyOptions);
    }

    [Test]
    public void Parse_JsonObjectToDefaultWithoutProperty_ReturnNoParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("{}");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new NoParseResult($"{JsonRpcConstants.ParamsProperty}.{PropertyName}");
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_JsonObjectToObject_ReturnSuccessParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("{}");
        var bindingStyle = BindingStyle.Object;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new SuccessParseResult(parameters.RootElement, JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected, JsonEquivalencyOptions);
    }

    [Test]
    public void Parse_JsonObjectToArray_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("{}");
        var bindingStyle = BindingStyle.Array;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new ErrorParseResult("Can't bind object to collection parameter", JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_JsonObjectToUnknownBindingStyle_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("{}");
        var bindingStyle = (BindingStyle) 3;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new ErrorParseResult($"Unknown bindingStyle [{bindingStyle}]", $"{JsonRpcConstants.ParamsProperty}.{PropertyName}");
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_JsonArrayToDefaultOutsideArray_ReturnNoParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("[]");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new NoParseResult($"{JsonRpcConstants.ParamsProperty}[{0}]");
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_JsonArrayToDefaultWithNullValue_ReturnNullParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("[ null ]");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new NullParseResult($"{JsonRpcConstants.ParamsProperty}[{0}]");
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_JsonArrayToDefaultWithValue_ReturnSuccessParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var value = "123";
        var parameters = JsonDocument.Parse($"[ {value} ]");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new SuccessParseResult(JsonDocument.Parse($"{value}").RootElement, $"{JsonRpcConstants.ParamsProperty}[{0}]");
        result.Should().BeEquivalentTo(expected, JsonEquivalencyOptions);
    }

    [Test]
    public void Parse_JsonArrayToObject_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("[]");
        var bindingStyle = BindingStyle.Object;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new ErrorParseResult("Can't bind array to object parameter", JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_JsonArrayToArray_ReturnSuccessParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("[]");
        var bindingStyle = BindingStyle.Array;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new SuccessParseResult(parameters.RootElement, JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected, JsonEquivalencyOptions);
    }

    [Test]
    public void Parse_JsonArrayToUnknownBindingStyle_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("[]");
        var bindingStyle = (BindingStyle) 3;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new ErrorParseResult($"Unknown bindingStyle [{bindingStyle}]", $"{JsonRpcConstants.ParamsProperty}[{0}]");
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NullJsonToDefault_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse($$"""{ "{{JsonRpcConstants.ParamsProperty}}": {} }""");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new ErrorParseResult("Can't bind method arguments from null json params", JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NullJsonToObject_ReturnNullParseResult()
    {
        var rawCall = JsonDocument.Parse($$"""{ "{{JsonRpcConstants.ParamsProperty}}": {} }""");
        var bindingStyle = BindingStyle.Object;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new NullParseResult(JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NullJsonToArray_ReturnNullParseResult()
    {
        var rawCall = JsonDocument.Parse($$"""{ "{{JsonRpcConstants.ParamsProperty}}": {} }""");
        var bindingStyle = BindingStyle.Array;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new NullParseResult(JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NullJsonToUnknownBindingStyle_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse($$"""{ "{{JsonRpcConstants.ParamsProperty}}": {} }""");
        var bindingStyle = (BindingStyle) 3;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new ErrorParseResult($"Unknown bindingStyle [{bindingStyle}]", JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NoJsonToDefault_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var bindingStyle = BindingStyle.Default;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new ErrorParseResult("Can't bind method arguments from missing json params", JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NoJsonToObject_ReturnNoParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var bindingStyle = BindingStyle.Object;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new NoParseResult(JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NoJsonToArray_ReturnNoParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var bindingStyle = BindingStyle.Array;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new NoParseResult(JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parse_NoJsonToUnknownBindingStyle_ReturnErrorParseResult()
    {
        var rawCall = JsonDocument.Parse("{}");
        var bindingStyle = (BindingStyle) 3;
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, bindingStyle, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, null, parameterMetadata);

        var expected = new ErrorParseResult($"Unknown bindingStyle [{bindingStyle}]", JsonRpcConstants.ParamsProperty);
        result.Should().BeEquivalentTo(expected);
    }

    [TestCase("\"str\"")]
    [TestCase("123")]
    [TestCase("true")]
    [TestCase("1.23")]
    public void Parse_UnsupportedJson_ReturnErrorParseResult(string json)
    {
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse(json);
        var parameterMetadata = new JsonRpcParameterMetadata(PropertyName, 0, BindingStyle.Default, false, "originalName", typeof(object));

        var result = paramsParser.Parse(rawCall, parameters, parameterMetadata);

        var expected = new ErrorParseResult($"Unsupported root JSON value kind: [{parameters.RootElement.ValueKind}]", string.Empty);
        result.Should().BeEquivalentTo(expected);
    }

    private static EquivalencyAssertionOptions<SuccessParseResult> JsonEquivalencyOptions(EquivalencyAssertionOptions<SuccessParseResult> options) =>
        options.Using(JsonElementEqualityComparer.Instance);

    private const string PropertyName = "propertyName";
}
