using System.Text.Json;

namespace Tochka.JsonRpc.Common;

/// <summary>
/// Extensions for common JSON-RPC logic
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Get item from collection by it's Type
    /// </summary>
    /// <param name="collection">Collection of objects</param>
    /// <typeparam name="T">Type of item to get</typeparam>
    /// <returns>First item of type <typeparamref name="T" /> if found, null otherwise</returns>
    public static T? Get<T>(this IEnumerable<object> collection)
        where T : class => collection.FirstOrDefault(static x => x is T) as T;

    /// <summary>
    /// Try to convert name by PropertyNamingPolicy
    /// </summary>
    /// <param name="jsonSerializerOptions">JSON serializer options to get PropertyNamingPolicy from</param>
    /// <param name="name">Name to convert</param>
    /// <returns>Converted name if PropertyNamingPolicy not null, initial name otherwise</returns>
    public static string ConvertName(this JsonSerializerOptions jsonSerializerOptions, string name) =>
        jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(name) ?? name;
}
