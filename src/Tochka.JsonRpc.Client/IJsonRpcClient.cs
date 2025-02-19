using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Models.BatchResult;
using Tochka.JsonRpc.Client.Models.SingleResult;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;

namespace Tochka.JsonRpc.Client;

/// <summary>
/// Interface for JSON-RPC client
/// </summary>
public interface IJsonRpcClient
{
    /// <summary>
    /// JsonSerializerOptions used to process JSON-RPC params/result/error.data. Does not affect JSON-RPC root object
    /// </summary>
    JsonSerializerOptions DataJsonSerializerOptions { get; }

    /// <summary>
    /// JsonSerializerOptions used to process JSON-RPC root object. Does not affect JSON-RPC params/result/error.data
    /// </summary>
    JsonSerializerOptions HeadersJsonSerializerOptions { get; }

    /// <summary>
    /// Send notification to given url. Does not return any data from server. Expects HTTP 200. Response body is ignored
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="notification">JSON-RPC notification</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task SendNotification<TParams>(string requestUrl, Notification<TParams> notification, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send notification to BaseUrl. Does not return any data from server. Expects HTTP 200. Response body is ignored
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="notification">JSON-RPC notification</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
    Task SendNotification<TParams>(Notification<TParams> notification, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send notification to given url. Does not return any data from server. Expects HTTP 200. Response body is ignored
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task SendNotification<TParams>(string requestUrl, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send notification to BaseUrl. Does not return any data from server. Expects HTTP 200. Response body is ignored
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
    Task SendNotification<TParams>(string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC Rpc response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest<TParams>(string requestUrl, Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult> SendRequest<TParams>(Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send request to given url. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest<TParams>(string requestUrl, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send request to BaseUrl. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult> SendRequest<TParams>(string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest<TParams>(string requestUrl, IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest<TParams>(IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class;

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC Rpc typed response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string requestUrl, Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class;

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class;

    /// <summary>
    /// Send request to given url. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string requestUrl, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class;

    /// <summary>
    /// Send request to BaseUrl. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class;

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string requestUrl, IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class;

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TParams">Type of params</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="parameters">JSON-RPC params - This member MAY be omitted</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class;

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC Rpc response
    /// </summary>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest(string requestUrl, Request request, CancellationToken cancellationToken);

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult> SendRequest(Request request, CancellationToken cancellationToken);

    /// <summary>
    /// Send request to given url. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest(string requestUrl, string method, CancellationToken cancellationToken);

    /// <summary>
    /// Send request to BaseUrl. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult> SendRequest(string method, CancellationToken cancellationToken);

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest(string requestUrl, IRpcId id, string method, CancellationToken cancellationToken);

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC response
    /// </summary>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult> SendRequest(IRpcId id, string method, CancellationToken cancellationToken);

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC Rpc typed response
    /// </summary>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string requestUrl, Request request, CancellationToken cancellationToken)
        where TResponse : class;

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="request">JSON-RPC request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(Request request, CancellationToken cancellationToken)
        where TResponse : class;

    /// <summary>
    /// Send request to given url. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string requestUrl, string method, CancellationToken cancellationToken)
        where TResponse : class;

    /// <summary>
    /// Send request to BaseUrl. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string method, CancellationToken cancellationToken)
        where TResponse : class;

    /// <summary>
    /// Send request to given url. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string requestUrl, IRpcId id, string method, CancellationToken cancellationToken)
        where TResponse : class;

    /// <summary>
    /// Send request to BaseUrl. Expects HTTP 200 with JSON-RPC typed response
    /// </summary>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <param name="id">JSON-RPC request id. Can be null</param>
    /// <param name="method">JSON-RPC method</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialized as batch response</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(IRpcId id, string method, CancellationToken cancellationToken)
        where TResponse : class;

    /// <summary>
    /// Send batch of requests or notifications to given url. Expects HTTP 200 with batch JSON-RPC response if batch contains at least one request
    /// </summary>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="calls">JSON-RPC requests or notifications</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body deserialized as single response, response count does not match requests in batch</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<IBatchJsonRpcResult?> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken);

    /// <summary>
    /// Send batch of requests or notifications to BaseUrl. Expects HTTP 200 with batch JSON-RPC response if batch contains at least one request
    /// </summary>
    /// <param name="calls">JSON-RPC requests or notifications</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body deserialized as single response, response count does not match requests in batch</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<IBatchJsonRpcResult?> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken);

    /// <summary>
    /// Send batch of requests or notifications to given url. Expects HTTP 200 with batch JSON-RPC typed response if batch contains at least one request
    /// </summary>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="calls">JSON-RPC requests or notifications</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body deserialized as single response, response count does not match requests in batch</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<IBatchJsonRpcResult<TResponse>?> SendBatch<TResponse>(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken);

    /// <summary>
    /// Send batch of requests or notifications to BaseUrl. Expects HTTP 200 with batch JSON-RPC typed response if batch contains at least one request
    /// </summary>
    /// <param name="calls">JSON-RPC requests or notifications</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse">Type of response</typeparam>
    /// <returns>Result to be inspected for response data or errors</returns>
    /// <exception cref="JsonRpcException">When HTTP status code is not 200, body deserialized as single response, response count does not match requests in batch</exception>
    /// <exception cref="JsonException">When reading or deserializing JSON from body failed</exception>
    /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
    Task<IBatchJsonRpcResult<TResponse>?> SendBatch<TResponse>(IEnumerable<ICall> calls, CancellationToken cancellationToken);

    /// <summary>
    /// Send request or notification to given url. Returns raw <see cref="HttpResponseMessage" /> without any checks or parsing
    /// </summary>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="call">JSON-RPC request or notification</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Raw HTTP response</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Call is official name")]
    Task<HttpResponseMessage> Send(string requestUrl, ICall call, CancellationToken cancellationToken);

    /// <summary>
    /// Send request or notification to BaseUrl. Returns raw <see cref="HttpResponseMessage" /> without any checks or parsing
    /// </summary>
    /// <param name="call">JSON-RPC request or notification</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Raw HTTP response</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Call is official name")]
    Task<HttpResponseMessage> Send(ICall call, CancellationToken cancellationToken);

    /// <summary>
    /// Send batch of requests or notifications to given url. Returns raw <see cref="HttpResponseMessage" /> without any checks or parsing
    /// </summary>
    /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
    /// <param name="calls">JSON-RPC requests or notifications</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Raw HTTP response</returns>
    Task<HttpResponseMessage> Send(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken);

    /// <summary>
    /// Send batch of requests or notifications to BaseUrl. Returns raw <see cref="HttpResponseMessage" /> without any checks or parsing
    /// </summary>
    /// <param name="calls">JSON-RPC requests or notifications</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Raw HTTP response</returns>
    Task<HttpResponseMessage> Send(IEnumerable<ICall> calls, CancellationToken cancellationToken);
}
