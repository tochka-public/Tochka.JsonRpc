using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

/// <inheritdoc />
/// <summary>
/// Wrapper for single requests
/// </summary>
/// <param name="Call">Single call from request</param>
[ExcludeFromCodeCoverage]
public sealed record SingleRequestWrapper
(
    JsonDocument Call
) : IRequestWrapper;
