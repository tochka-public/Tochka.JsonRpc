namespace Tochka.JsonRpc.Common;

public static class Extensions
{
    public static T? Get<T>(this IEnumerable<object> collection)
        where T : class => collection.FirstOrDefault(static x => x is T) as T;
}
