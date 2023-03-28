using System.Text.Json;

namespace Tochka.JsonRpc.Common;

public static class Utils
{
    public static JsonDocument? SerializeParams<TParams>(TParams data, JsonSerializerOptions serializerOptions)
        where TParams : class?
    {
        if (data == null)
        {
            return null;
        }

        var serialized = JsonSerializer.SerializeToDocument(data, serializerOptions);
        var jsonValueKind = serialized.RootElement.ValueKind;
        if (jsonValueKind is JsonValueKind.Object or JsonValueKind.Array)
        {
            return serialized;
        }

        throw new InvalidOperationException($"Expected params [{typeof(TParams).Name}] to be serializable into object or array, got [{jsonValueKind}]");
    }

    public static T? DeserializeErrorData<T>(JsonDocument? data, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        if (data == null)
        {
            // data may be omitted - if data was not present at all, do not throw
            return default;
        }

        try
        {
            return data.Deserialize<T>(dataJsonSerializerOptions);
        }
        catch (Exception e) when (e is JsonException or NotSupportedException)
        {
            // if data serializer failed: maybe this is server error, try header serializer
            return data.Deserialize<T>(headersJsonSerializerOptions);
        }
    }
}
