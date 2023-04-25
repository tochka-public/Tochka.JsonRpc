using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

internal class ParameterBinder : IParameterBinder
{
    public void SetResult(ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata, IParseResult parseResult, JsonSerializerOptions jsonSerializerOptions)
    {
        var parameterName = bindingContext.ModelMetadata.Name!;
        switch (parseResult)
        {
            case SuccessParseResult successParseResult:
                SetSuccessResult(bindingContext, parameterName, successParseResult, jsonSerializerOptions);
                break;

            case ErrorParseResult errorParseResult:
                SetError(bindingContext, parameterName, $"Error while binding value by JSON key = [{errorParseResult.JsonKey}] - {errorParseResult.Message}");
                break;

            case NoParseResult when parameterMetadata.IsOptional:
                // key was not in json but is optional parameter
                // log.LogTrace("{methodName}, parameterName {parameterName}: [{parseResult}]",
                //     nameof(SetNoResult),
                //     parameterName,
                //     noParseResult);
                break;

            case NoParseResult noParseResult:
                // key was not in json and it isn't optional parameter
                SetError(bindingContext, parameterName, $"Bind value not found (expected JSON key = [{noParseResult.JsonKey}])");
                break;

            case NullParseResult nullParseResult when bindingContext.ModelMetadata is { IsReferenceOrNullableType: true, IsRequired: true }:
                // json value was null and type can be null but value required
                // log.LogTrace("{methodName}, parameterName {parameterName}: [{parseResult}]",
                //     nameof(SetNullResult),
                //     parameterName,
                //     nullParseResult);

                SetError(bindingContext, parameterName, $"Can not bind value = [null] by JSON key = [{nullParseResult.JsonKey}] to required parameter");
                break;

            case NullParseResult when bindingContext.ModelMetadata is { IsReferenceOrNullableType: true }:
                // json value was null and type can be null
                // log.LogTrace("{methodName}, parameterName {parameterName}: [{parseResult}]",
                //     nameof(SetNullResult),
                //     parameterName,
                //     nullParseResult);

                bindingContext.Result = ModelBindingResult.Success(null);
                break;

            case NullParseResult nullParseResult:
                // json value was null and type can not be null
                var error = $"Can not bind value = [null] by JSON key = [{nullParseResult.JsonKey}] to non-nullable parameter of type [{bindingContext.ModelMetadata.ModelType.Name}]";
                SetError(bindingContext, parameterName, error);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(parseResult), parseResult.GetType().Name);
        }
    }

    private static void SetSuccessResult(ModelBindingContext bindingContext, string parameterName, SuccessParseResult parseResult, JsonSerializerOptions jsonSerializerOptions)
    {
        try
        {
            // log.LogTrace("{methodName}, parameterName {parameterName}: [{result}]",
            //     nameof(SetResultSafe),
            //     parameterName,
            //     result);

            // log.LogTrace("{methodName} deserializing json to [{modelTypeName}]", nameof(SetDeserializedResult), context.ModelMetadata.ModelType.Name);

            bindingContext.Result = ModelBindingResult.Success(parseResult.Value.Deserialize(bindingContext.ModelMetadata.ModelType, jsonSerializerOptions));
        }
        catch (Exception e)
        {
            // log.LogWarning(e,
            //     "{methodName}, parameterName {parameterName} failed result was [{result}]",
            //     nameof(SetResultSafe),
            //     parameterName,
            //     result);

            var error = $"Error while binding value = [{parseResult.Value}] (JSON key = [{parseResult.JsonKey}]) - {e.GetType().Name}: {e.Message}";
            SetError(bindingContext, parameterName, error);
        }
    }

    private static void SetError(ModelBindingContext bindingContext, string parameterName, string error)
    {
        // log.LogTrace("{methodName}, parameterName {parameterName}: [{parseResult}]",
        //     nameof(SetError),
        //     parameterName,
        //     parseResult);
        //
        bindingContext.ModelState.AddModelError(parameterName, error);
    }
}
