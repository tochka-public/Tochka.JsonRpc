using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client
{
    public interface IJsonRpcClient
    {
        /// <summary>
        /// Send notification to given url. Does not return any data from server. Expects HTTP 200. Body is ignored
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
        /// <param name="notification">JSON Rpc notification</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task SendNotification<T>(string requestUrl, Notification<T> notification, CancellationToken cancellationToken);

        /// <summary>
        /// Send notification to BaseUrl. Does not return any data from server. Expects HTTP 200. Body is ignored
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="notification">JSON Rpc notification</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
        Task SendNotification<T>(Notification<T> notification, CancellationToken cancellationToken);

        /// <summary>
        /// Send notification to given url. Does not return any data from server. Expects HTTP 200. Body is ignored
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
        /// <param name="method">JSON Rpc method</param>
        /// <param name="parameters">JSON Rpc params</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task SendNotification<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Send notification to BaseUrl. Does not return any data from server. Expects HTTP 200. Body is ignored
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="method">JSON Rpc method</param>
        /// <param name="parameters">JSON Rpc params</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200</exception>
        Task SendNotification<T>(string method, T parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Send request to given url. Expects HTTP 200 with JSON Rpc response
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
        /// <param name="request">JSON Rpc request</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialzed as batch response</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, Request<T> request, CancellationToken cancellationToken);

        /// <summary>
        /// Send request to BaseUrl. Expects HTTP 200 with JSON Rpc response
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="request">JSON Rpc request</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialzed as batch response</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        Task<ISingleJsonRpcResult> SendRequest<T>(Request<T> request, CancellationToken cancellationToken);

        /// <summary>
        /// Send request to given url. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON Rpc response
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
        /// <param name="method">JSON Rpc method</param>
        /// <param name="parameters">JSON Rpc params</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialzed as batch response</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Send request to BaseUrl. Id is generated with IJsonRpcIdGenerator. Expects HTTP 200 with JSON Rpc response
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="method">JSON Rpc method</param>
        /// <param name="parameters">JSON Rpc params</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialzed as batch response</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        Task<ISingleJsonRpcResult> SendRequest<T>(string method, T parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Send request to given url. Expects HTTP 200 with JSON Rpc response
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
        /// <param name="id">JSON Rpc request id. Can be null</param>
        /// <param name="method">JSON Rpc method</param>
        /// <param name="parameters">JSON Rpc params</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialzed as batch response</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, IRpcId id, string method, T parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Send request to BaseUrl. Expects HTTP 200 with JSON Rpc response
        /// </summary>
        /// <typeparam name="T">Type of params</typeparam>
        /// <param name="id">JSON Rpc request id. Can be null</param>
        /// <param name="method">JSON Rpc method</param>
        /// <param name="parameters">JSON Rpc params</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body is empty or deserialzed as batch response</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task<ISingleJsonRpcResult> SendRequest<T>(IRpcId id, string method, T parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Send batch of requests or notifications to given url. Expects HTTP 200 with batch JSON Rpc response if batch contains at least one request
        /// </summary>
        /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
        /// <param name="calls">JSON Rpc requests or notifications</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body deserialzed as single response, response count does not match requests in batch</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task<IBatchJsonRpcResult> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken);

        /// <summary>
        /// Send batch of requests or notifications to BaseUrl. Expects HTTP 200 with batch JSON Rpc response if batch contains at least one request
        /// </summary>
        /// <param name="calls">JSON Rpc requests or notifications</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result to be inspected for response data or errors</returns>
        /// <exception cref="JsonRpcException">When HTTP status code is not 200, body deserialzed as single response, response count does not match requests in batch</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">When reading or deserializing JSON from body failed</exception>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task<IBatchJsonRpcResult> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken);

        /// <summary>
        /// Send request or notification to given url. Does not check or parse HTTP response
        /// </summary>
        /// <param name="requestUrl">Relative path, appended to BaseAddress. Must not start with '/'</param>
        /// <param name="call">JSON Rpc request or notification</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Raw HTTP response</returns>
        /// <exception cref="System.ArgumentException">When requestUrl starts with '/'</exception>
        Task<HttpResponseMessage> Send(string requestUrl, ICall call, CancellationToken cancellationToken);

        /// <summary>
        /// Send request or notification to BaseUrl. Does not check or parse HTTP response
        /// </summary>
        /// <param name="call">JSON Rpc request or notification</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Raw HTTP response</returns>
        Task<HttpResponseMessage> Send(ICall call, CancellationToken cancellationToken);

        /// <summary>
        /// Serializer used to process JSON Rpc params. Does not affect JSON Rpc root object
        /// </summary>
        IRpcSerializer Serializer { get; }
    }
}