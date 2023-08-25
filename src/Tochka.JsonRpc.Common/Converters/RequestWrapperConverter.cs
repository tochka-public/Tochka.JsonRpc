using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Common.Converters;

/// <inheritdoc />
/// <summary>
/// Deserialize request to single or batch from object/array
/// </summary>
[PublicAPI]
public class RequestWrapperConverter : JsonConverter<IRequestWrapper>
{
    // NOTE: used in server to parse requests, no need for serialization
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IRequestWrapper value, JsonSerializerOptions options) =>
        throw new InvalidOperationException();

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault", Justification = "Other cases not allowed for request wrappers")]
    public override IRequestWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tokenType = reader.TokenType;
        return tokenType switch
        {
            JsonTokenType.StartObject => new SingleRequestWrapper(JsonSerializer.Deserialize<JsonDocument>(ref reader, options)!),
            JsonTokenType.StartArray => new BatchRequestWrapper(JsonSerializer.Deserialize<List<JsonDocument>>(ref reader, options)!),
            _ => throw new JsonRpcFormatException($"Expected {{}} or [] as root element. Got {tokenType}")
        };
    }
}
