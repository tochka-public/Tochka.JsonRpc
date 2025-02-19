using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

/// <inheritdoc />
/// <summary>
/// Parse result indicating that error occured while parsing
/// </summary>
/// <param name="Message">Error message</param>
/// <param name="JsonKey">JSON key of parameter that was parsed</param>
[ExcludeFromCodeCoverage]
public sealed record ErrorParseResult
(
    string Message,
    string JsonKey
) : IParseResult;
