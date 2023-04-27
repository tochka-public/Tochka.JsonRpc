using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

internal class JsonRpcParamsParser : IJsonRpcParamsParser
{
    public IParseResult Parse(JsonDocument rawCall, JsonDocument? parameters, JsonRpcParameterMetadata parameterMetadata)
    {
        var bindingStyle = parameterMetadata.BindingStyle;
        var jsonValueKind = parameters?.RootElement.ValueKind;
        var hasParamsNode = rawCall.RootElement.TryGetProperty(JsonRpcConstants.ParamsProperty, out _);
        return jsonValueKind switch
        {
            JsonValueKind.Object => ParseObject(parameters!.RootElement, parameterMetadata.PropertyName, bindingStyle),
            JsonValueKind.Array => ParseArray(parameters!.RootElement, parameterMetadata.Position, bindingStyle),
            null when hasParamsNode => ParseNull(bindingStyle),
            null => ParseNoParams(bindingStyle),
            _ => new ErrorParseResult($"Unsupported root JSON value kind: [{jsonValueKind}]", string.Empty)
        };
    }

    private static IParseResult ParseObject(JsonElement json, string jsonProperty, BindingStyle bindingStyle)
    {
        var jsonKey = $"{JsonRpcConstants.ParamsProperty}.{jsonProperty}";
        return bindingStyle switch
        {
            // map keys to args by names
            BindingStyle.Default when json.TryGetProperty(jsonProperty, out var value) => value.ValueKind == JsonValueKind.Null
                ? new NullParseResult(jsonKey)
                : new SuccessParseResult(value, jsonKey),
            BindingStyle.Default => new NoParseResult(jsonKey),
            // map 1:1 to object
            BindingStyle.Object =>
                // log.LogTrace("Binding whole json object to [{jsonProperty}]", jsonProperty);
                new SuccessParseResult(json, jsonKey),
            BindingStyle.Array =>
                // log.LogTrace("Can not bind json object to array for [{jsonProperty}]", jsonProperty);
                new ErrorParseResult("Can not bind object to collection parameter", JsonRpcConstants.ParamsProperty),
            _ => new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", jsonKey)
        };
    }

    private static IParseResult ParseArray(JsonElement json, int index, BindingStyle bindingStyle)
    {
        var jsonKey = $"{JsonRpcConstants.ParamsProperty}[{index}]";
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // map array items to args by indices
                if (index >= json.GetArrayLength())
                {
                    return new NoParseResult(jsonKey);
                }

                var value = json[index];
                if (value.ValueKind == JsonValueKind.Null)
                {
                    // log.LogTrace("Json value for binding by index [{indexString}] is null", indexString);
                    return new NullParseResult(jsonKey);
                }

                // log.LogTrace("Json value for binding by index [{indexString}] found", indexString);
                return new SuccessParseResult(value, jsonKey);

                // log.LogTrace("Json value for binding by index [{indexString}] not found", indexString);
            case BindingStyle.Object:
                // log.LogWarning("Can not bind json array to object for [{indexString}]", indexString);
                return new ErrorParseResult("Can not bind array to object parameter", JsonRpcConstants.ParamsProperty);
            case BindingStyle.Array:
                // map 1:1 to collection
                // log.LogTrace("Binding whole json array to [{indexString}]", indexString);
                return new SuccessParseResult(json, jsonKey);
            default:
                // log.LogWarning("Binding failed for index [{indexString}]", indexString);
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", jsonKey);
        }
    }

    private static IParseResult ParseNull(BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // log.LogWarning("Binding null to regular parameter failed");
                return new ErrorParseResult("Can not bind method arguments from null json params", JsonRpcConstants.ParamsProperty);
            case BindingStyle.Object:
                // this is fine
                // log.LogTrace("Binding null to object or array parameter");
                return new NullParseResult(JsonRpcConstants.ParamsProperty);
            case BindingStyle.Array:
                // this is fine
                // log.LogTrace("Binding null to object or array parameter");
                return new NullParseResult(JsonRpcConstants.ParamsProperty);
            default:
                // log.LogWarning("Binding null failed");
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", JsonRpcConstants.ParamsProperty);
        }
    }

    private static IParseResult ParseNoParams(BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                return new ErrorParseResult("Can not bind method arguments from missing json params", JsonRpcConstants.ParamsProperty);
            case BindingStyle.Object:
                // this is fine
                // log.LogTrace("Binding null to object or array parameter");
                return new NoParseResult(JsonRpcConstants.ParamsProperty);
            case BindingStyle.Array:
                // this is fine
                // log.LogTrace("Binding null to object or array parameter");
                return new NoParseResult(JsonRpcConstants.ParamsProperty);
            default:
                // log.LogWarning("Binding null failed");
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", JsonRpcConstants.ParamsProperty);
        }
    }
}
