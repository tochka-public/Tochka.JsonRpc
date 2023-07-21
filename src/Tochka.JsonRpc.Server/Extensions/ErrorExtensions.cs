using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;

namespace Tochka.JsonRpc.Server.Extensions;

/// <summary>
/// Extensions for <see cref="IError" />
/// </summary>
[PublicAPI]
public static class ErrorExtensions
{
    /// <summary>
    /// Throws special <see cref="JsonRpcErrorException" /> which is converted into JSON-RPC error response with given code, message and data
    /// </summary>
    /// <param name="error">Error to return in response</param>
    /// <exception cref="JsonRpcErrorException">Exception with error</exception>
    [DoesNotReturn]
    public static void ThrowAsException(this IError error) => throw new JsonRpcErrorException(error);

    /// <summary>
    /// Serialize error.data
    /// </summary>
    /// <param name="error">Error with data to serialize</param>
    /// <param name="jsonSerializerOptions">Data serializer options</param>
    /// <returns>Error with same code and message but serialized data</returns>
    public static Error<JsonDocument> AsUntypedError(this IError error, JsonSerializerOptions jsonSerializerOptions)
    {
        var data = error.Data == null
            ? null
            : JsonSerializer.SerializeToDocument(error.Data, jsonSerializerOptions);
        return new Error<JsonDocument>(error.Code, error.Message, data);
    }
}
