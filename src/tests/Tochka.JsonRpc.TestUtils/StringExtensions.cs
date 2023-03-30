namespace Tochka.JsonRpc.TestUtils;

public static class StringExtensions
{
    public static string TrimAllLines(this string str) =>
        string.Join("\n", str.Split("\n").Select(static l => l.Trim()));
}
