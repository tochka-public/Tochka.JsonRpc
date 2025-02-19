using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

/// <inheritdoc />
/// <summary>
/// Parse result indicating that there was no member with given key
/// </summary>
/// <param name="JsonKey">JSON key of parameter that was parsed</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record NoParseResult
(
    string JsonKey
) : IParseResult;
