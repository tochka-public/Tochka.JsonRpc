using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Common.Converters;

public class ResponseConverter : JsonConverter<IResponse>
{
    // NOTE: used in client to parse responses, no need for serialization
    public override void Write(Utf8JsonWriter writer, IResponse value, JsonSerializerOptions options) => throw new InvalidOperationException();

    public override IResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        CheckProperties(reader) switch
        {
            // "Id is REQUIRED. If there was an error in detecting the id in the Request object (e.g. Parse error/Invalid Request), it MUST be Null."
            // (JsonTokenType.Null, not actual null)
            { HasId: false } => throw new ArgumentException($"JSON Rpc response does not have [{JsonRpcConstants.IdProperty}] property"),
            { HasResult: true, HasError: false } => JsonSerializer.Deserialize<UntypedResponse>(ref reader, options),
            { HasResult: false, HasError: true } => JsonSerializer.Deserialize<UntypedErrorResponse>(ref reader, options),
            var properties => throw new ArgumentException($"JSON Rpc response is invalid, expected one of properties. Has [{JsonRpcConstants.ResultProperty}]: {properties.HasResult}. Has [{JsonRpcConstants.ErrorProperty}]: {properties.HasError}")
        };

    private static PropertiesInfo CheckProperties(Utf8JsonReader propertyReader)
    {
        var hasId = false;
        var hasError = false;
        var hasResult = false;
        var initialDepth = propertyReader.CurrentDepth;
        while (propertyReader.Read())
        {
            var tokenType = propertyReader.TokenType;
            var currentDepth = propertyReader.CurrentDepth;
            if (tokenType == JsonTokenType.EndObject && currentDepth == initialDepth)
            {
                break;
            }

            if (tokenType is JsonTokenType.StartObject or JsonTokenType.StartArray && currentDepth == initialDepth + 1)
            {
                propertyReader.Skip();
                continue;
            }

            if (tokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = propertyReader.GetString();
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
            }
        }

        return new PropertiesInfo(hasId, hasResult, hasError);
    }

    private record PropertiesInfo(bool HasId, bool HasResult, bool HasError);
}
