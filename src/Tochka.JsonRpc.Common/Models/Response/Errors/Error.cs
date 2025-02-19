using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Response.Errors;

/// <inheritdoc />
/// <summary>
/// Error with typed data
/// </summary>
/// <param name="Code">Number that indicates the error type that occurred</param>
/// <param name="Message">Short description of the error</param>
/// <param name="Data">Additional information about the error</param>
/// <typeparam name="TData">Type of data</typeparam>
[PublicAPI]
[ExcludeFromCodeCoverage]
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
public record Error<TData>
(
    int Code,
    string Message,
    TData? Data
) : IError
{
    object? IError.Data => Data;
}
