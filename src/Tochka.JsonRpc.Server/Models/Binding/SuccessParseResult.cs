using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Server.Models.Binding;

/// <summary>
/// Indicate that json had no-null value at given key
/// </summary>
[ExcludeFromCodeCoverage]
public class SuccessParseResult : IParseResult
{
    public string Key { get; }

    public JsonElement Value { get; }

    public SuccessParseResult(JsonElement? value, string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override string ToString()
    {
        return $"Bind value is [{Value.ValueKind}]. Json key [{Key}]";
    }
}
