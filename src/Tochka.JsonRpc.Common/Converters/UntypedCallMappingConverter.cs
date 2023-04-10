using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Converters;

public class UntypedCallMappingConverter : JsonConverter<IUntypedCall>
{
    // NOTE: used in client to serialize requests, no need for serialization
    public override IUntypedCall Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new InvalidOperationException();

    // System.Text.Json can't serialize derived types:
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-6-0#serialize-properties-of-derived-classes
    public override void Write(Utf8JsonWriter writer, IUntypedCall value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case UntypedNotification notification:
                JsonSerializer.Serialize(writer, notification, options);
                break;
            case UntypedRequest request:
                JsonSerializer.Serialize(writer, request, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
        }
    }
}
