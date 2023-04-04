using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Models.Binding;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

public class ParamsParser : IParamsParser
{
    private readonly ILogger log;

    public ParamsParser(ILogger<ParamsParser> log)
    {
        this.log = log;
    }

    public IParseResult ParseParams(JsonDocument jsonParams, ParameterMetadata parameterMetadata)
    {
        var bindingStyle = parameterMetadata.BindingStyle;
        var jsonValueKind = jsonParams?.RootElement.ValueKind;
        switch (jsonValueKind)
        {
            case JsonValueKind.Object:
                return ParseObject(jsonParams, parameterMetadata.Name.Json, bindingStyle);
            case JsonValueKind.Array:
                return ParseArray(jsonParams, parameterMetadata.Index, bindingStyle);
            case JsonValueKind.Null or null:
                return ParseNull(bindingStyle);
            default:
                return new ErrorParseResult($"Unsupported root JSON element type: [{jsonValueKind}]", string.Empty);
        }
    }

    protected internal virtual IParseResult ParseObject(JsonDocument jsonDocument, string jsonProperty, BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // map keys to args by names
                if (jsonDocument.RootElement.TryGetProperty(jsonProperty, out var value))
                {
                    if (value.ValueKind == JsonValueKind.Null)
                    {
                        log.LogTrace("Json value for binding [{jsonProperty}] is null", jsonProperty);
                        return new NullParseResult(jsonProperty);
                    }

                    log.LogTrace("Json value for binding [{jsonProperty}] found", jsonProperty);
                    return new SuccessParseResult(value, jsonProperty);
                }

                log.LogTrace("Json value for binding [{jsonProperty}] not found", jsonProperty);
                return new NoParseResult(jsonProperty);
            case BindingStyle.Object:
                // map 1:1 to object
                log.LogTrace("Binding whole json object to [{jsonProperty}]", jsonProperty);
                return new SuccessParseResult(jsonDocument.RootElement, jsonProperty);
            case BindingStyle.Array:
                log.LogTrace("Can not bind json object to array for [{jsonProperty}]", jsonProperty);
                return new ErrorParseResult("Can not bind object to collection parameter", jsonProperty);
            default:
                log.LogWarning("Binding failed for [{jsonProperty}]", jsonProperty);
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", jsonProperty);
        }
    }

    protected internal virtual IParseResult ParseArray(JsonDocument jsonArray, int index, BindingStyle bindingStyle)
    {
        var indexString = index.ToString(CultureInfo.InvariantCulture);
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                // map array items to args by indices
                if (index < jsonArray.RootElement.GetArrayLength())
                {
                    var value = jsonArray.RootElement[index];
                    if (value.ValueKind == JsonValueKind.Null)
                    {
                        log.LogTrace("Json value for binding by index [{indexString}] is null", indexString);
                        return new NullParseResult(indexString);
                    }

                    log.LogTrace("Json value for binding by index [{indexString}] found", indexString);
                    return new SuccessParseResult(value, indexString);
                }

                log.LogTrace("Json value for binding by index [{indexString}] not found", indexString);
                return new NoParseResult(indexString);
            case BindingStyle.Object:
                log.LogWarning("Can not bind json array to object for [{indexString}]", indexString);
                return new ErrorParseResult("Can not bind array to object parameter", indexString);
            case BindingStyle.Array:
                // map 1:1 to collection
                log.LogTrace("Binding whole json array to [{indexString}]", indexString);
                return new SuccessParseResult(jsonArray.RootElement, indexString);
            default:
                log.LogWarning("Binding failed for index [{indexString}]", indexString);
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", indexString);
        }
    }

    protected internal virtual IParseResult ParseNull(BindingStyle bindingStyle)
    {
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                log.LogWarning("Binding null to regular parameter failed");
                // TODO: what if all params have default value?
                return new ErrorParseResult("Can not bind method arguments from [null] json params", string.Empty);
            case BindingStyle.Object:
            case BindingStyle.Array:
                // this is fine
                log.LogTrace("Binding null to object or array parameter");
                return new NullParseResult(string.Empty);
            default:
                log.LogWarning("Binding null failed");
                return new ErrorParseResult($"Unknown {nameof(bindingStyle)} [{bindingStyle}]", string.Empty);
        }
    }
}
