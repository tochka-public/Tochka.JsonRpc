using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Client.Models.BatchResult;

/// <summary>
/// Result of batch JSON-RPC request
/// </summary>
[PublicAPI]
public interface IBatchJsonRpcResult
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
    /// Check if response has error
    /// </summary>
    /// <param name="id">Response id</param>
    /// <exception cref="JsonRpcException">if no response with given id or response has error</exception>
    bool HasError(IRpcId id);

    /// <summary>
    /// Advanced data for complex work with batch Result of JSON-RPC request
    /// </summary>
    /// <returns></returns>
    IBatchJsonRpcResultAdvanced Advanced { get; init; }
}

/// <summary>
/// Result of batch JSON-RPC request with typed response
/// <typeparam name="TResponse">Type of response</typeparam>
/// </summary>
[PublicAPI]
public interface IBatchJsonRpcResult<out TResponse>
{
    /// <summary>
    /// Try to deserialize response result to typed response or throw if unable
    /// </summary>
    /// <param name="id">Response id</param>
    /// <exception cref="JsonRpcException">if no response with given id or response has error</exception>
    TResponse? GetResponseOrThrow(IRpcId id);

    /// <summary>
    /// Get deserialized to typed response result
    /// </summary>
    /// <param name="id">Response id</param>
    /// <returns>Result or null if no response with given id or response has error</returns>
    TResponse? AsResponse(IRpcId id);

    /// <summary>
    /// Check if response has error
    /// </summary>
    /// <param name="id">Response id</param>
    /// <exception cref="JsonRpcException">if no response with given id or response has error</exception>
    bool HasError(IRpcId id);

    /// <summary>
    /// Advanced data for complex work with batch Result of JSON-RPC request
    /// </summary>
    /// <returns></returns>
    IBatchJsonRpcResultAdvanced Advanced { get; init; }
}
