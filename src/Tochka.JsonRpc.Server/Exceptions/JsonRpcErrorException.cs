using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Exceptions;

[SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения")]
public class JsonRpcErrorException : Exception
{
    public IError Error { get; }

    public JsonRpcErrorException(IError error) => Error = error;
}
