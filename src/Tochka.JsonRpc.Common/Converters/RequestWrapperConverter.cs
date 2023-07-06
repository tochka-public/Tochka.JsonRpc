﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Common.Converters;

/// <inheritdoc />
/// <summary>
/// Handle dumb rule of request being single or batch
/// </summary>
public class RequestWrapperConverter : JsonConverter<IRequestWrapper>
{
    // NOTE: used in server to parse requests, no need for serialization
    public override void Write(Utf8JsonWriter writer, IRequestWrapper value, JsonSerializerOptions options) =>
        throw new InvalidOperationException();

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
