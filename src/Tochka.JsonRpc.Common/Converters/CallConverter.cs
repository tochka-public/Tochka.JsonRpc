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
        HasId(reader)
            ? JsonSerializer.Deserialize<UntypedRequest>(ref reader, options)
            : JsonSerializer.Deserialize<UntypedNotification>(ref reader, options);

    private static bool HasId(Utf8JsonReader propertyReader)
    {
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
            if (propertyName == JsonRpcConstants.IdProperty)
            {
                return true;
            }
        }

        return false;
    }

    // public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    // {
    //     var jObject = JObject.Load(reader);
    //     var idProperty = jObject[JsonRpcConstants.IdProperty];
    //     var idType = idProperty?.Type;
    //     switch (idType)
    //     {
    //         case JTokenType.String:
    //         case JTokenType.Integer:
    //         case JTokenType.Null:
    //             var idValue = idProperty as JValue;
    //             var result1 = jObject.ToObject<UntypedRequest>(serializer);
    //             result1.RawJson = jObject.ToString();
    //             result1.RawId = idValue;
    //             return result1;
    //
    //         case null:
    //             var result2 = jObject.ToObject<UntypedNotification>(serializer);
    //             result2.RawJson = jObject.ToString();
    //             return result2;
    //
    //         default:
    //             throw new ArgumentOutOfRangeException(nameof(idType), idType, "Expected string, number, null or nothing as Id");
    //     }
    // }
}
