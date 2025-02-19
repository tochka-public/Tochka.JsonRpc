using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

/// <inheritdoc />
/// <summary>
/// Wrapper for single responses
/// </summary>
/// <param name="Response">Single response object</param>
[ExcludeFromCodeCoverage]
public sealed record SingleResponseWrapper
(
    IResponse Response
) : IResponseWrapper;
