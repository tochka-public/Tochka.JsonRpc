using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Errors;

/// <summary>
/// Server-defined details about exceptions and unexpected HTTP codes
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ExceptionInfo(string Type, string Message, object? Details);
