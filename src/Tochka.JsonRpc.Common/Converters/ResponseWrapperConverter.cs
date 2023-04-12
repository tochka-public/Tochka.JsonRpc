using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Common.Converters;

/// <inheritdoc />
/// <summary>
/// Handle dumb rule of response being single on some batch errors
/// </summary>
public class ResponseWrapperConverter : JsonConverter<IResponseWrapper>
{
    // System.Text.Json can't serialize derived types:
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-6-0#serialize-properties-of-derived-classes
    public override void Write(Utf8JsonWriter writer, IResponseWrapper value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case SingleResponseWrapper singleResponseWrapper:
                JsonSerializer.Serialize(writer, singleResponseWrapper, options);
                break;
            case BatchResponseWrapper batchResponseWrapper:
                JsonSerializer.Serialize(writer, batchResponseWrapper, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
        }
    }

    [SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault", Justification = "Other cases not allowed for response wrappers")]
    public override IResponseWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tokenType = reader.TokenType;
        return tokenType switch
        {
            JsonTokenType.StartObject => new SingleResponseWrapper(JsonSerializer.Deserialize<IResponse>(ref reader, options)!),
            JsonTokenType.StartArray => new BatchResponseWrapper(JsonSerializer.Deserialize<List<IResponse>>(ref reader, options)!),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Expected {} or [] as root element")
        };
    }
}
