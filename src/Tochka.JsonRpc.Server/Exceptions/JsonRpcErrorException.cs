using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Exceptions;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Method is required")]
public class JsonRpcErrorException : Exception
{
    public IError Error { get; }

    public JsonRpcErrorException(IError error) => Error = error;
}
