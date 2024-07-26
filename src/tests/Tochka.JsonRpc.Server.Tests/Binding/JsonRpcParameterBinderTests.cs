using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Server.Tests.Binding;

[TestFixture]
public class JsonRpcParameterBinderTests
{
    private JsonRpcParameterBinder parameterBinder;

    [SetUp]
    public void Setup() => parameterBinder = new JsonRpcParameterBinder(Mock.Of<ILogger<JsonRpcParameterBinder>>());

    [Test]
    public void SetResult_SuccessParseResult_SetSuccess()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var result = 123;
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(result.GetType(), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new SuccessParseResult(JsonSerializer.SerializeToElement(result, jsonSerializerOptions), "jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        var expected = ModelBindingResult.Success(result);
        bindingContext.Result.Should().BeEquivalentTo(expected);
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Valid);
    }

    [Test]
    public void SetResult_SuccessParseResultAndDeserializationError_SetError()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var result = 123;
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(IEnumerable<int>), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new SuccessParseResult(JsonSerializer.SerializeToElement(result, jsonSerializerOptions), "jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        var expected = ModelBindingResult.Failed();
        bindingContext.Result.Should().BeEquivalentTo(expected);
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Invalid);
    }

    [Test]
    public void SetResult_ErrorParseResult_SetError()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(int), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new ErrorParseResult("message", "jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        var expected = ModelBindingResult.Failed();
        bindingContext.Result.Should().BeEquivalentTo(expected);
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Invalid);
    }

    [Test]
    public void SetResult_NoParseResultAndParameterIsOptional_DontSetResult()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, true, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(int), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new NoParseResult("jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        bindingContext.Result.IsModelSet.Should().BeFalse();
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Valid);
    }

    [Test]
    public void SetResult_NoParseResultAndParameterIsntOptional_SetError()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(int), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new NoParseResult("jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        var expected = ModelBindingResult.Failed();
        bindingContext.Result.Should().BeEquivalentTo(expected);
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Invalid);
    }

    [Test]
    public void SetResult_NullParseResultAndParameterIsRequired_SetError()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(string), ParameterName, true),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new NullParseResult("jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        var expected = ModelBindingResult.Failed();
        bindingContext.Result.Should().BeEquivalentTo(expected);
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Invalid);
    }

    [Test]
    public void SetResult_NullParseResultAndParameterCanBeNull_SetSuccess()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(string), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new NullParseResult("jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        var expected = ModelBindingResult.Success(null);
        bindingContext.Result.Should().BeEquivalentTo(expected);
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Valid);
    }

    [Test]
    public void SetResult_NullParseResultAndParameterCantBeNull_SetError()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(int), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = new NullParseResult("jsonKey");

        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        var expected = ModelBindingResult.Failed();
        bindingContext.Result.Should().BeEquivalentTo(expected);
        bindingContext.ModelState.ValidationState.Should().Be(ModelValidationState.Invalid);
    }

    [Test]
    public void SetResult_UnknownResult_Throw()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ModelMetadata = new FakeModelMetadata(typeof(int), ParameterName),
            ModelState = new ModelStateDictionary()
        };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var parseResult = Mock.Of<IParseResult>();

        var action = () => parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    private const string ParameterName = "parameterName";
}
