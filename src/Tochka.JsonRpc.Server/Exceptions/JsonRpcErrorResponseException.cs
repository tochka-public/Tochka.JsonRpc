using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Exceptions;

/// <summary>
/// Special exception which is converted into response with given code, message and data
/// </summary>
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Error is required")]
public class JsonRpcErrorResponseException : Exception
{
    public IError Error { get; }

    public JsonRpcErrorResponseException(IError error)
    {
        Error = error;
    }
}