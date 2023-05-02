using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

internal class JsonRpcParamsParser : IJsonRpcParamsParser
{
    private readonly ILogger<JsonRpcParamsParser> log;

    public JsonRpcParamsParser(ILogger<JsonRpcParamsParser> log) => this.log = log;

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

    private IParseResult ParseObject(JsonElement json, string jsonProperty, BindingStyle bindingStyle)
    {
        var jsonKey = $"{JsonRpcConstants.ParamsProperty}.{jsonProperty}";
        switch (bindingStyle)
        {
            // map keys to args by names when they exist
            case BindingStyle.Default when json.TryGetProperty(jsonProperty, out var value):
                log.LogTrace("Found [{jsonValueKind}] for [{jsonProperty}]", value.ValueKind, jsonProperty);
                return value.ValueKind == JsonValueKind.Null
                    ? new NullParseResult(jsonKey)
                    : new SuccessParseResult(value, jsonKey);

            case BindingStyle.Default:
                log.LogTrace("Property [{jsonProperty}] not found", jsonProperty);
                return new NoParseResult(jsonKey);

            // map 1:1 to object
            case BindingStyle.Object:
                log.LogTrace("Binding whole json params object to [{jsonProperty}]", jsonProperty);
                return new SuccessParseResult(json, jsonKey);

            case BindingStyle.Array:
                log.LogTrace("Can't bind json params object to collection for [{jsonProperty}]", jsonProperty);
                return new ErrorParseResult("Can not bind object to collection parameter", JsonRpcConstants.ParamsProperty);

            default:
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", jsonKey);
        }
    }

    private IParseResult ParseArray(JsonElement json, int index, BindingStyle bindingStyle)
    {
        var jsonKey = $"{JsonRpcConstants.ParamsProperty}[{index}]";
        switch (bindingStyle)
        {
            // map array items to args by indices
            case BindingStyle.Default:
                var paramsLength = json.GetArrayLength();
                if (index >= paramsLength)
                {
                    log.LogTrace("Value by index [{index}] not found - params have only {paramsLength} elements", index, paramsLength);
                    return new NoParseResult(jsonKey);
                }

                var value = json[index];
                if (value.ValueKind == JsonValueKind.Null)
                {
                    log.LogTrace("Json value for binding by index [{index}] is null", index);
                    return new NullParseResult(jsonKey);
                }

                log.LogTrace("Json value for binding by index [{index}] found", index);
                return new SuccessParseResult(value, jsonKey);

            case BindingStyle.Object:
                log.LogWarning("Can not bind json params array to object for argument by index [{index}]", index);
                return new ErrorParseResult("Can not bind array to object parameter", JsonRpcConstants.ParamsProperty);

            // map 1:1 to collection
            case BindingStyle.Array:
                log.LogTrace("Binding whole json params array to [{index}]", index);
                return new SuccessParseResult(json, jsonKey);

            default:
                log.LogWarning("Binding failed for argument by index [{index}]", index);
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", jsonKey);
        }
    }

    private IParseResult ParseNull(BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            // can't get properties for binding to arguments from null params
            case BindingStyle.Default:
                log.LogWarning("Binding null to regular argument failed");
                return new ErrorParseResult("Can not bind method arguments from null json params", JsonRpcConstants.ParamsProperty);

            // will bind successfully if object can be null
            case BindingStyle.Object:
                log.LogTrace("Binding null to object argument");
                return new NullParseResult(JsonRpcConstants.ParamsProperty);

            // will bind successfully if collection can be null
            case BindingStyle.Array:
                log.LogTrace("Binding null to collection argument");
                return new NullParseResult(JsonRpcConstants.ParamsProperty);

            default:
                log.LogWarning("Binding null failed");
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", JsonRpcConstants.ParamsProperty);
        }
    }

    private IParseResult ParseNoParams(BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            // can't get properties for binding to arguments from missing params
            case BindingStyle.Default:
                return new ErrorParseResult("Can not bind method arguments from missing json params", JsonRpcConstants.ParamsProperty);

            // will bind successfully if object has default value specified
            case BindingStyle.Object:
                log.LogTrace("Binding nothing to object argument");
                return new NoParseResult(JsonRpcConstants.ParamsProperty);

            // will bind successfully if collection has default value specified
            case BindingStyle.Array:
                log.LogTrace("Binding nothing to collection argument");
                return new NoParseResult(JsonRpcConstants.ParamsProperty);

            default:
                log.LogWarning("Binding nothing failed");
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", JsonRpcConstants.ParamsProperty);
        }
    }
}
