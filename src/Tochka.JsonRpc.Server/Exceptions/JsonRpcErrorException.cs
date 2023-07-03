using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Exceptions;

[ExcludeFromCodeCoverage]
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Error is required")]
public class JsonRpcErrorException : Exception
{
    public IError Error { get; }

    public JsonRpcErrorException(IError error) => Error = error;
}
