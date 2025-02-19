using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

/// <inheritdoc />
/// <summary>
/// Parse result indicating that error occured while parsing
/// </summary>
/// <param name="Message">Error message</param>
/// <param name="JsonKey">JSON key of parameter that was parsed</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record ErrorParseResult
(
    string Message,
    string JsonKey
) : IParseResult;
