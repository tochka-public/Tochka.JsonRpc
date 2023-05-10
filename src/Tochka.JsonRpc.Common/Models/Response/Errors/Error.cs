using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Errors;

[ExcludeFromCodeCoverage]
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
public record Error<T>(int Code, string Message, T? Data) : IError
{
    object? IError.Data => Data;
}
