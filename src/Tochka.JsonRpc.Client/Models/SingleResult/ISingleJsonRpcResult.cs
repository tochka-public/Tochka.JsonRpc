namespace Tochka.JsonRpc.Client.Models.SingleResult;

/// <summary>
/// Result of single JSON-RPC request
/// </summary>
public interface ISingleJsonRpcResult
{
    /// <summary>
    /// Advanced data for complex work with single Result of JSON-RPC request
    /// </summary>
    /// <returns></returns>
    ISingleJsonRpcResultAdvanced Advanced { get; init; }

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
}

/// <summary>
/// Result of single JSON-RPC request with typed response
/// <typeparam name="TResponse">Type of response</typeparam>
/// </summary>
public interface ISingleJsonRpcResult<out TResponse>
{
    /// <summary>
    /// Advanced data for complex work with single Result of JSON-RPC request
    /// </summary>
    /// <returns></returns>
    ISingleJsonRpcResultAdvanced Advanced { get; init; }

    /// <summary>
    /// Try to deserialize response result to typed response or throw if unable
    /// </summary>
    /// <exception cref="JsonRpcException">if response has error</exception>
    TResponse? GetResponseOrThrow();

    /// <summary>
    /// Get deserialized to typed response result
    /// </summary>
    /// <returns>Result or null if response has error</returns>
    TResponse? AsResponse();

    /// <summary>
    /// Check if response has error
    /// </summary>
    bool HasError();
}
