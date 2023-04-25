using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Converters;

/// <summary>
///     Handle dumb rule of Id present for requests and not present for notifications
/// </summary>
public class CallConverter : JsonConverter<IUntypedCall>
{
    // NOTE: used in server to parse requests, no need for serialization
    public override void Write(Utf8JsonWriter writer, IUntypedCall value, JsonSerializerOptions options) =>
        throw new InvalidOperationException();

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

    private record PropertiesInfo(bool HasId, bool HasMethod, bool HasVersion);
}
