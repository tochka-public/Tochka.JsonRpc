using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

/// <inheritdoc />
/// <summary>
/// Wrapper for single responses
/// </summary>
/// <param name="Response">Single response object</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record SingleResponseWrapper
(
    IResponse Response
) : IResponseWrapper;
