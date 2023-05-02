using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;

namespace Tochka.JsonRpc.Server.Extensions;

public static class ErrorExtensions
{
    /// <summary>
    /// Throws special <see cref="JsonRpcErrorException"/> which is converted into response with given code, message and data
    /// </summary>
    /// <param name="error"></param>
    /// <exception cref="JsonRpcErrorException"></exception>
    public static void ThrowAsException(this IError error) => throw new JsonRpcErrorException(error);

    public static Error<JsonDocument> AsUntypedError(this IError error, JsonSerializerOptions jsonSerializerOptions)
    {
        var data = error.Data == null
            ? null
            : JsonSerializer.SerializeToDocument(error.Data, jsonSerializerOptions);
        return new Error<JsonDocument>(error.Code, error.Message, data);
    }
}
