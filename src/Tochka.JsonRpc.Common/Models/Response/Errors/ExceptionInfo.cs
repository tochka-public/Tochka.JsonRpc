using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Errors;

/// <summary>
/// Server-defined details about exceptions
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ExceptionInfo
(
    string Type,
    string Message,
    object? Details
);
