using System.Text.Json;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

internal class JsonRpcParamsParser : IJsonRpcParamsParser
{
    public IParseResult Parse(JsonDocument? parameters, JsonRpcParameterMetadata parameterMetadata)
    {
        var bindingStyle = parameterMetadata.BindingStyle;
        var jsonValueKind = parameters?.RootElement.ValueKind;
        return jsonValueKind switch
        {
            JsonValueKind.Object => ParseObject(parameters!.RootElement, parameterMetadata.PropertyName, bindingStyle),
            JsonValueKind.Array => ParseArray(parameters!.RootElement, parameterMetadata.Position, bindingStyle),
            null => ParseNull(bindingStyle),
            _ => new ErrorParseResult($"Unsupported root JSON value kind: [{jsonValueKind}]")
        };
    }

    private static IParseResult ParseObject(JsonElement json, string jsonProperty, BindingStyle bindingStyle) =>
        bindingStyle switch
        {
            // map keys to args by names
            BindingStyle.Default when json.TryGetProperty(jsonProperty, out var value) => value.ValueKind == JsonValueKind.Null
                ? new NullParseResult()
                : new SuccessParseResult(value),
            BindingStyle.Default => new NoParseResult(),
            // map 1:1 to object
            BindingStyle.Object =>
                // log.LogTrace("Binding whole json object to [{jsonProperty}]", jsonProperty);
                new SuccessParseResult(json),
            BindingStyle.Array =>
                // log.LogTrace("Can not bind json object to array for [{jsonProperty}]", jsonProperty);
                new ErrorParseResult("Can not bind object to collection parameter"),
            _ => new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]")
        };

    private static IParseResult ParseArray(JsonElement json, int index, BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // map array items to args by indices
                if (index < json.GetArrayLength())
                {
                    var value = json[index];
                    if (value.ValueKind == JsonValueKind.Null)
                    {
                        // log.LogTrace("Json value for binding by index [{indexString}] is null", indexString);
                        return new NullParseResult();
                    }

                    // log.LogTrace("Json value for binding by index [{indexString}] found", indexString);
                    return new SuccessParseResult(value);
                }

                // log.LogTrace("Json value for binding by index [{indexString}] not found", indexString);
                return new NoParseResult();
            case BindingStyle.Object:
                // log.LogWarning("Can not bind json array to object for [{indexString}]", indexString);
                return new ErrorParseResult("Can not bind array to object parameter");
            case BindingStyle.Array:
                // map 1:1 to collection
                // log.LogTrace("Binding whole json array to [{indexString}]", indexString);
                return new SuccessParseResult(json);
            default:
                // log.LogWarning("Binding failed for index [{indexString}]", indexString);
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]");
        }
    }

    private static IParseResult ParseNull(BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // log.LogWarning("Binding null to regular parameter failed");
                return new ErrorParseResult("Can not bind method arguments from [null] json params");
            case BindingStyle.Object:
            case BindingStyle.Array:
                // this is fine
                // log.LogTrace("Binding null to object or array parameter");
                return new NullParseResult();
            default:
                // log.LogWarning("Binding null failed");
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]");
        }
    }
}
