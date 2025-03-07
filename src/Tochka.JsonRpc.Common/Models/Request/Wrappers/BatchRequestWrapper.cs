﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

/// <inheritdoc />
/// <summary>
/// Wrapper for batch requests
/// </summary>
/// <param name="Calls">List of calls in batch request</param>
[ExcludeFromCodeCoverage]
public sealed record BatchRequestWrapper
(
    List<JsonDocument> Calls
) : IRequestWrapper;
