using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Errors;

/// <summary>
/// Server-defined details about exceptions and unexpected HTTP codes
/// </summary>
[ExcludeFromCodeCoverage]
public record ExceptionInfo(string Type, string Message, int? InternalHttpCode, object Details);
