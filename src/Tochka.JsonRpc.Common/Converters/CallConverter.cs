using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Converters;

/// <inheritdoc />
/// <summary>
/// Deserialize calls to request or notification based on existing/missing id property
/// </summary>
[PublicAPI]
public class CallConverter : JsonConverter<IUntypedCall>
{
    // System.Text.Json can't serialize derived types:
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-6-0#serialize-properties-of-derived-classes
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IUntypedCall value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case UntypedRequest untypedRequest:
                JsonSerializer.Serialize(writer, untypedRequest, options);
                break;
            case UntypedNotification untypedNotification:
                JsonSerializer.Serialize(writer, untypedNotification, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
        }
    }

    /// <inheritdoc />
    public override IUntypedCall? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        CheckProperties(reader) switch
        {
            { HasMethod: false } => throw new JsonRpcFormatException($"JSON Rpc call does not have [{JsonRpcConstants.MethodProperty}] property"),
            { HasVersion: false } => throw new JsonRpcFormatException($"JSON Rpc call does not have [{JsonRpcConstants.JsonrpcVersionProperty}] property"),
            { HasId: false } => JsonSerializer.Deserialize<UntypedNotification>(ref reader, options),
            _ => JsonSerializer.Deserialize<UntypedRequest>(ref reader, options)
        };

    private static PropertiesInfo CheckProperties(Utf8JsonReader propertyReader)
    {
        var hasId = false;
        var hasMethod = false;
        var hasVersion = false;
        foreach (var propertyName in Utils.GetPropertyNames(ref propertyReader))
        {
            switch (propertyName)
            {
                case JsonRpcConstants.IdProperty:
                    hasId = true;
                    break;
                case JsonRpcConstants.MethodProperty:
                    hasMethod = true;
                    break;
                case JsonRpcConstants.JsonrpcVersionProperty:
                    hasVersion = true;
                    break;
            }
        }

        return new PropertiesInfo(hasId, hasMethod, hasVersion);
    }

    private record PropertiesInfo
    (
        bool HasId,
        bool HasMethod,
        bool HasVersion
    );
}
