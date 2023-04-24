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
            case SuccessParseResult successBindResult:
                SetSuccessResult(bindingContext, parameterName, successBindResult, jsonSerializerOptions);
                break;

            case ErrorParseResult errorBindResult:
                SetError(bindingContext, parameterName, errorBindResult);
                break;

            case NoParseResult noParseResult when parameterMetadata.IsOptional:
                // key was not in json but is optional parameter
                // log.LogTrace("{methodName}, parameterName {parameterName}: [{parseResult}]",
                //     nameof(SetNoResult),
                //     parameterName,
                //     noParseResult);
                break;

            case NoParseResult noBindResult:
                // key was not in json and is required
                SetError(bindingContext, parameterName, noBindResult);
                break;

            case NullParseResult nullParseResult when bindingContext.ModelMetadata.IsReferenceOrNullableType:
                // json value was null and type can be null
                // log.LogTrace("{methodName}, parameterName {parameterName}: [{parseResult}]",
                //     nameof(SetNullResult),
                //     parameterName,
                //     nullParseResult);

                bindingContext.Result = ModelBindingResult.Success(null);
                break;

            case NullParseResult nullParseResult:
                // json value was null and type can not be null
                var error = new ErrorParseResult($"Can not bind null json value to non-nullable parameter of type [{bindingContext.ModelMetadata.ModelType.Name}]");
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

            bindingContext.ModelState.AddModelError(parameterName, $"{parseResult}. {e.GetType().Name}: {e.Message}");
        }
    }

    private static void SetError(ModelBindingContext context, string parameterName, IParseResult parseResult)
    {
        // log.LogTrace("{methodName}, parameterName {parameterName}: [{parseResult}]",
        //     nameof(SetError),
        //     parameterName,
        //     parseResult);
        //
        context.ModelState.AddModelError(parameterName, parseResult.ToString());
    }
}
