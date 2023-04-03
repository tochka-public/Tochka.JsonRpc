using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Models.Binding;

/// <summary>
/// Indicate that error occured
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorParseResult : IParseResult
{
    public string Key { get; }

    public string Message { get; }

    public ErrorParseResult(string message, string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public override string ToString()
    {
        return $"Bind error. {Message}. Json key [{Key}]";
    }
}