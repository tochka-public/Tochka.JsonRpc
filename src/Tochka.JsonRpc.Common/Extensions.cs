using System.Text.Json;

namespace Tochka.JsonRpc.Common;

public static class Extensions
{
    public static T? Get<T>(this IEnumerable<object> collection)
        where T : class => collection.FirstOrDefault(static x => x is T) as T;

    public static string ConvertName(this JsonSerializerOptions jsonSerializerOptions, string name) =>
        jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(name) ?? name;
}
