using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tochka.JsonRpc.OpenRpc.Converters;

/// <inheritdoc />
/// <summary>
/// Serialize Uri without escaping template parameters
/// </summary>
internal class UriConverter : JsonConverter<Uri>
{
    // NOTE: used to serialize OpenRPC document, no need for deserialization
    public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new InvalidOperationException();

    // STJ uses Uri.OriginalString out of the box, but it is escaped and unreadable in documentation
    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
