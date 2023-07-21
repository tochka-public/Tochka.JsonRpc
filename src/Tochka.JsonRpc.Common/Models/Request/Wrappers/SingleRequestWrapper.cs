using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

/// <inheritdoc />
/// <summary>
/// Wrapper for single requests
/// </summary>
/// <param name="Call">Single call from request</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record SingleRequestWrapper(JsonDocument Call) : IRequestWrapper;
