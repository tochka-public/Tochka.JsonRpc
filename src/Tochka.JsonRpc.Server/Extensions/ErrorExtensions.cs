using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;

namespace Tochka.JsonRpc.Server.Extensions;

public static class ErrorExtensions
{
    /// <summary>
    /// Throws special JsonRpcErrorResponseException which is converted into response with given code, message and data
    /// </summary>
    /// <param name="error"></param>
    /// <exception cref="JsonRpcErrorException"></exception>
    public static void ThrowAsException(this IError error)
    {
        throw new JsonRpcErrorException(error);
    }
}
