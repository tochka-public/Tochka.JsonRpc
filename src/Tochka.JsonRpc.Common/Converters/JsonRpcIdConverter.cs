using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Converters;

/// <inheritdoc />
/// <summary>
/// Convert id to and from string/number/null for requests and responses
/// </summary>
[PublicAPI]
public class JsonRpcIdConverter : JsonConverter<IRpcId>
{
    /// <inheritdoc />
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IRpcId value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case NumberRpcId numberRpcId:
                writer.WriteNumberValue(numberRpcId.Value);
                break;
            case StringRpcId stringRpcId:
                writer.WriteStringValue(stringRpcId.Value);
                break;
            case NullRpcId:
                writer.WriteNullValue();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
        }
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault", Justification = "Other cases not allowed for id property")]
    public override IRpcId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var idType = reader.TokenType;
        return idType switch
        {
            JsonTokenType.String => new StringRpcId(reader.GetString()!),
            JsonTokenType.Number when reader.TryGetInt64(out var number) => new NumberRpcId(number),
            JsonTokenType.Null => new NullRpcId(),
            _ => throw new JsonRpcFormatException($"Expected string, number or null as Id. Got {idType}")
        };
    }
}
