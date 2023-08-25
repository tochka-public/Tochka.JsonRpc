using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Common.Converters;

/// <inheritdoc />
/// <summary>
/// Deserialize response that can have Either result or error
/// </summary>
[PublicAPI]
public class ResponseConverter : JsonConverter<IResponse>
{
    // System.Text.Json can't serialize derived types:
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-6-0#serialize-properties-of-derived-classes
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IResponse value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case UntypedResponse untypedResponse:
                JsonSerializer.Serialize(writer, untypedResponse, options);
                break;
            case UntypedErrorResponse untypedErrorResponse:
                JsonSerializer.Serialize(writer, untypedErrorResponse, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
        }
    }

    /// <inheritdoc />
    public override IResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        CheckProperties(reader) switch
        {
            // "Id is REQUIRED. If there was an error in detecting the id in the Request object (e.g. Parse error/Invalid Request), it MUST be Null."
            // (JsonTokenType.Null, not actual null)
            { HasId: false } => throw new JsonRpcFormatException($"JSON Rpc response does not have [{JsonRpcConstants.IdProperty}] property"),
            { HasVersion: false } => throw new JsonRpcFormatException($"JSON Rpc response does not have [{JsonRpcConstants.JsonrpcVersionProperty}] property"),
            { HasResult: true, HasError: false } => JsonSerializer.Deserialize<UntypedResponse>(ref reader, options),
            { HasResult: false, HasError: true } => JsonSerializer.Deserialize<UntypedErrorResponse>(ref reader, options),
            var properties => throw new JsonRpcFormatException($"JSON Rpc response is invalid, expected one of properties. Has [{JsonRpcConstants.ResultProperty}]: {properties.HasResult}. Has [{JsonRpcConstants.ErrorProperty}]: {properties.HasError}")
        };

    private static PropertiesInfo CheckProperties(Utf8JsonReader propertyReader)
    {
        var hasId = false;
        var hasError = false;
        var hasResult = false;
        var hasVersion = false;
        foreach (var propertyName in Utils.GetPropertyNames(ref propertyReader))
        {
            switch (propertyName)
            {
                case JsonRpcConstants.IdProperty:
                    hasId = true;
                    break;
                case JsonRpcConstants.ResultProperty:
                    hasResult = true;
                    break;
                case JsonRpcConstants.ErrorProperty:
                    hasError = true;
                    break;
                case JsonRpcConstants.JsonrpcVersionProperty:
                    hasVersion = true;
                    break;
            }
        }

        return new PropertiesInfo(hasId, hasResult, hasError, hasVersion);
    }

    private record PropertiesInfo(bool HasId, bool HasResult, bool HasError, bool HasVersion);
}
