using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.Server.Binding;

internal class JsonRpcParameterBinder : IJsonRpcParameterBinder
{
    private readonly ILogger<JsonRpcParameterBinder> log;

    public JsonRpcParameterBinder(ILogger<JsonRpcParameterBinder> log) => this.log = log;

    public void SetResult(ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata, IParseResult parseResult, JsonSerializerOptions jsonSerializerOptions)
    {
        var parameterName = bindingContext.ModelMetadata.Name!;
        switch (parseResult)
        {
            case SuccessParseResult successParseResult:
                log.LogTrace("[{parameterName}]: {parseResult}", parameterName, successParseResult);
                SetSuccessResult(bindingContext, parameterName, successParseResult, jsonSerializerOptions);
                break;

            case ErrorParseResult errorParseResult:
                log.LogTrace("[{parameterName}]: {parseResult}", parameterName, errorParseResult);
                var error = $"Error while binding value by JSON key = [{errorParseResult.JsonKey}] - {errorParseResult.Message}";
                SetError(bindingContext, parameterName, error);
                break;

            // key was not in json but is optional parameter
            // optional == has default value
            case NoParseResult noParseResult when parameterMetadata.IsOptional:
                log.LogTrace("[{parameterName}] - optional: {parseResult}", parameterName, noParseResult);
                break;

            // key was not in json and it isn't optional parameter
            case NoParseResult noParseResult:
                log.LogTrace("[{parameterName}]: {parseResult}", parameterName, noParseResult);
                SetError(bindingContext, parameterName, $"Bind value not found (expected JSON key = [{noParseResult.JsonKey}])");
                break;

            // json value was null and type can be null but it is required parameter
            // value required == no `?` for NRT or [Required] attribute
            case NullParseResult nullParseResult when bindingContext.ModelMetadata is { IsReferenceOrNullableType: true, IsRequired: true }:
                log.LogTrace("[{parameterName}] - can be null, required: {parseResult}", parameterName, nullParseResult);
                var requiredError = $"Can't bind value = [null] by JSON key = [{nullParseResult.JsonKey}] to required parameter";
                SetError(bindingContext, parameterName, requiredError);
                break;

            // json value was null and type can be null and it isn't required parameter
            case NullParseResult nullParseResult when bindingContext.ModelMetadata is { IsReferenceOrNullableType: true }:
                log.LogTrace("[{parameterName}] - can be null: {parseResult}", parameterName, nullParseResult);
                bindingContext.Result = ModelBindingResult.Success(null);
                break;

            // json value was null and type can't be null
            case NullParseResult nullParseResult:
                log.LogTrace("[{parameterName}]: {parseResult}", parameterName, nullParseResult);
                var nullError = $"Can't bind value = [null] by JSON key = [{nullParseResult.JsonKey}] to non-nullable parameter of type [{bindingContext.ModelMetadata.ModelType.Name}]";
                SetError(bindingContext, parameterName, nullError);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(parseResult), parseResult.GetType().Name);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to wrap all unexpected parsing exceptions in binding error")]
    private void SetSuccessResult(ModelBindingContext bindingContext, string parameterName, SuccessParseResult parseResult, JsonSerializerOptions jsonSerializerOptions)
    {
        try
        {
            log.LogTrace("Deserializing json to [{modelTypeName}]", bindingContext.ModelMetadata.ModelType.Name);
            bindingContext.Result = ModelBindingResult.Success(parseResult.Value.Deserialize(bindingContext.ModelMetadata.ModelType, jsonSerializerOptions));
        }
        catch (Exception e)
        {
            log.LogWarning(e,
                "Failed deserializing [{parameterName}] to [{modelTypeName}] from json:\n{json}",
                parameterName,
                bindingContext.ModelMetadata.ModelType.Name,
                parseResult.Value);
            var error = $"Error while binding value = [{parseResult.Value}] (JSON key = [{parseResult.JsonKey}]) - {e.GetType().Name}: {e.Message}";
            SetError(bindingContext, parameterName, error);
        }
    }

    private static void SetError(ModelBindingContext bindingContext, string parameterName, string error) =>
        bindingContext.ModelState.AddModelError(parameterName, error);
}
