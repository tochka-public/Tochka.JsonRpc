using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

/// <inheritdoc />
/// <summary>
/// Wrapper for batch responses
/// </summary>
/// <param name="Responses">List of response objects</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record BatchResponseWrapper(List<IResponse> Responses) : IResponseWrapper;
