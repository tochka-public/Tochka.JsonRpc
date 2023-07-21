using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Client.Models;

/// <summary>
/// Result of single JSON-RPC request
/// </summary>
[PublicAPI]
public interface ISingleJsonRpcResult
{
    /// <summary>
    /// Try to deserialize response result or throw if unable
    /// </summary>
    /// <typeparam name="TResponse">Type to deserialize result to</typeparam>
    /// <exception cref="JsonRpcException">if response has error</exception>
    TResponse? GetResponseOrThrow<TResponse>();

    /// <summary>
    /// Get deserialized response result
    /// </summary>
    /// <typeparam name="TResponse">Type to deserialize result to</typeparam>
    /// <returns>Result or null if response has error</returns>
    TResponse? AsResponse<TResponse>();

    /// <summary>
    /// Check if response has error
    /// </summary>
    bool HasError();

    /// <summary>
    /// Get error without deserializing data
    /// </summary>
    /// <returns>Error or null if response has no error</returns>
    Error<JsonDocument>? AsAnyError();

    /// <summary>
    /// Get error with deserialized data
    /// </summary>
    /// <typeparam name="TError">Type to deserialize error.data to</typeparam>
    /// <returns>Error or null if response has no error</returns>
    Error<TError>? AsTypedError<TError>();

    /// <summary>
    /// Get error with deserialized ExceptionInfo data
    /// </summary>
    /// <returns>Error or null if response has no error</returns>
    Error<ExceptionInfo>? AsErrorWithExceptionInfo();
}
