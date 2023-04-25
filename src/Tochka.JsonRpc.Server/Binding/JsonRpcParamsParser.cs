using System.Globalization;
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
            null => ParseNull(bindingStyle, parameterMetadata),
            _ => new ErrorParseResult($"Unsupported root JSON value kind: [{jsonValueKind}]", string.Empty)
        };
    }

    private static IParseResult ParseObject(JsonElement json, string jsonProperty, BindingStyle bindingStyle) =>
        bindingStyle switch
        {
            // map keys to args by names
            BindingStyle.Default when json.TryGetProperty(jsonProperty, out var value) => value.ValueKind == JsonValueKind.Null
                ? new NullParseResult(jsonProperty)
                : new SuccessParseResult(value, jsonProperty),
            BindingStyle.Default => new NoParseResult(jsonProperty),
            // map 1:1 to object
            BindingStyle.Object =>
                // log.LogTrace("Binding whole json object to [{jsonProperty}]", jsonProperty);
                new SuccessParseResult(json, jsonProperty),
            BindingStyle.Array =>
                // log.LogTrace("Can not bind json object to array for [{jsonProperty}]", jsonProperty);
                new ErrorParseResult("Can not bind object to collection parameter", jsonProperty),
            _ => new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", jsonProperty)
        };

    private static IParseResult ParseArray(JsonElement json, int index, BindingStyle bindingStyle)
    {
        var indexString = index.ToString(CultureInfo.InvariantCulture);
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // map array items to args by indices
                if (index >= json.GetArrayLength())
                {
                    return new NoParseResult(indexString);
                }

                var value = json[index];
                if (value.ValueKind == JsonValueKind.Null)
                {
                    // log.LogTrace("Json value for binding by index [{indexString}] is null", indexString);
                    return new NullParseResult(indexString);
                }

                // log.LogTrace("Json value for binding by index [{indexString}] found", indexString);
                return new SuccessParseResult(value, indexString);

                // log.LogTrace("Json value for binding by index [{indexString}] not found", indexString);
            case BindingStyle.Object:
                // log.LogWarning("Can not bind json array to object for [{indexString}]", indexString);
                return new ErrorParseResult("Can not bind array to object parameter", indexString);
            case BindingStyle.Array:
                // map 1:1 to collection
                // log.LogTrace("Binding whole json array to [{indexString}]", indexString);
                return new SuccessParseResult(json, indexString);
            default:
                // log.LogWarning("Binding failed for index [{indexString}]", indexString);
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", indexString);
        }
    }

    private static IParseResult ParseNull(BindingStyle bindingStyle, JsonRpcParameterMetadata parameterMetadata)
    {
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // log.LogWarning("Binding null to regular parameter failed");
                return new ErrorParseResult("Can not bind method arguments from null or missing json params", string.Empty);
            case BindingStyle.Object:
                // this is fine
                // log.LogTrace("Binding null to object or array parameter");
                return new NoParseResult(parameterMetadata.PropertyName);
            case BindingStyle.Array:
                // this is fine
                // log.LogTrace("Binding null to object or array parameter");
                return new NoParseResult(parameterMetadata.Position.ToString(CultureInfo.InvariantCulture));
            default:
                // log.LogWarning("Binding null failed");
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", string.Empty);
        }
    }
}
