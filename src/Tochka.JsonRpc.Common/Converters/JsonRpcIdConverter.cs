using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Converters;

/// <inheritdoc />
/// <summary>
/// Handle dumb rule of Id as string/number/null for requests and responses
/// </summary>
public class JsonRpcIdConverter : JsonConverter<IRpcId>
{
    public override void Write(Utf8JsonWriter writer, IRpcId value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case NumberRpcId numberRpcId:
                writer.WriteNumberValue(numberRpcId.NumberValue);
                break;
            case StringRpcId stringRpcId:
                writer.WriteStringValue(stringRpcId.StringValue);
                break;
            case null:
                writer.WriteNullValue();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
        }
    }

    [SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault", Justification = "Other cases not allowed for id property")]
    public override IRpcId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var idType = reader.TokenType;
        return idType switch
        {
            JsonTokenType.String => new StringRpcId(reader.GetString()!),
            JsonTokenType.Number when reader.TryGetInt64(out var number) => new NumberRpcId(number),
            JsonTokenType.Null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(idType), idType, "Expected string, number or null as Id")
        };
    }
}
