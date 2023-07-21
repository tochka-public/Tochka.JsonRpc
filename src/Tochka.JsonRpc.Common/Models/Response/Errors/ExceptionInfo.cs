using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Response.Errors;

/// <summary>
/// Server-defined details about exceptions
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record ExceptionInfo(string Type, string Message, object? Details);
