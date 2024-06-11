using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Client.Models.Batch;

/// <summary>
/// Advanced data for complex work with batch Result of JSON-RPC request
/// </summary>
[PublicAPI]
public interface IBatchJsonRpcResultAdvanced
{
    /// <summary>
    /// Try to deserialize response result or throw if unable
    /// </summary>
    /// <param name="id">Response id</param>
    /// <typeparam name="TResponse">Type to deserialize result to</typeparam>
    /// <exception cref="JsonRpcException">if no response with given id or response has error</exception>
    TResponse? GetResponseOrThrow<TResponse>(IRpcId id);

    /// <summary>
    /// Get deserialized response result
    /// </summary>
    /// <param name="id">Response id</param>
    /// <typeparam name="TResponse">Type to deserialize result to</typeparam>
    /// <returns>Result or null if no response with given id or response has error</returns>
    TResponse? AsResponse<TResponse>(IRpcId id);
    
    /// <summary>
    /// Get error without deserializing data
    /// </summary>
    /// <param name="id">Response id</param>
    /// <returns>Error or null if no response with given id or response has no error</returns>
    Error<JsonDocument>? AsAnyError(IRpcId id);

    /// <summary>
    /// Get error with deserialized data
    /// </summary>
    /// <param name="id">Response id</param>
    /// <typeparam name="TError">Type to deserialize error.data to</typeparam>
    /// <returns>Error or null if no response with given id or response has no error</returns>
    Error<TError>? AsTypedError<TError>(IRpcId id);

    /// <summary>
    /// Get error with deserialized ExceptionInfo data
    /// </summary>
    /// <param name="id">Response id</param>
    /// <returns>Error or null if no response with given id or response has no error</returns>
    Error<ExceptionInfo>? AsErrorWithExceptionInfo(IRpcId id);
}