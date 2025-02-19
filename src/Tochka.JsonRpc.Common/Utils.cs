using System.Text.Json;

namespace Tochka.JsonRpc.Common;

/// <summary>
/// Helpers for common JSON-RPC logic
/// </summary>
public static class Utils
{
    /// <summary>
    /// Try to deserialize data first with data serializer options and then with "headers" serializer options
    /// </summary>
    /// <param name="data">Data to deserialize</param>
    /// <param name="headersJsonSerializerOptions">"Headers" serializer options</param>
    /// <param name="dataJsonSerializerOptions">Data serializer options</param>
    /// <typeparam name="T">Type to deserialize data to</typeparam>
    /// <returns>null if data is null, deserialized data otherwise</returns>
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

    /// <summary>
    /// Try to serialize params if it is not null and has correct JSON kind
    /// </summary>
    /// <param name="data">params to serialize</param>
    /// <param name="serializerOptions">Data serializer options</param>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <returns>null if params is null, serialized params otherwise</returns>
    /// <exception cref="InvalidOperationException">If params JSON kind neither object nor array</exception>
    internal static JsonDocument? SerializeParams<TParams>(TParams data, JsonSerializerOptions serializerOptions)
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

    /// <summary>
    /// Enumerate top level JSON object properties names
    /// </summary>
    /// <param name="propertyReader">Reader to read JSON object from</param>
    /// <returns>Names of top level properties</returns>
    internal static IEnumerable<string?> GetPropertyNames(ref Utf8JsonReader propertyReader)
    {
        // It's for forbidden to use Utf8JsonReader with yield return
        var propertyNames = new List<string?>();
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

            propertyNames.Add(propertyReader.GetString());
        }

        return propertyNames;
    }
}
