using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

/// <inheritdoc />
/// <summary>
/// Parse result indicating that value was null
/// </summary>
/// <param name="JsonKey">JSON key of parameter that was parsed</param>
[ExcludeFromCodeCoverage]
public sealed record NullParseResult
(
    string JsonKey
) : IParseResult;
