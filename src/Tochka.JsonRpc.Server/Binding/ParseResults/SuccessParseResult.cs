﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

/// <inheritdoc />
/// <summary>
/// Parse result indicating that value was successfully parsed
/// </summary>
/// <param name="Value">Parsed value</param>
/// <param name="JsonKey">JSON key of parameter that was parsed</param>
[ExcludeFromCodeCoverage]
public sealed record SuccessParseResult
(
    JsonElement Value,
    string JsonKey
) : IParseResult;
