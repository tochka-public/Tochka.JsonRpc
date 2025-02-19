using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

/// <inheritdoc />
/// <summary>
/// Wrapper for batch responses
/// </summary>
/// <param name="Responses">List of response objects</param>
[ExcludeFromCodeCoverage]
public sealed record BatchResponseWrapper
(
    List<IResponse> Responses
) : IResponseWrapper;
