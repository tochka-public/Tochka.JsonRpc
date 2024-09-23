using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Models.BatchResult;
using Tochka.JsonRpc.Client.Models.SingleResult;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Client;

/// <inheritdoc />
/// <summary>
/// Base class for JSON Rpc clients
/// </summary>
[PublicAPI]
public abstract class JsonRpcClientBase : IJsonRpcClient
{
    /// <summary>
    /// Value of User-Agent header in http request
    /// </summary>
    /// <remarks>
    /// Default is "Tochka.JsonRpc.Client"
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public virtual string UserAgent => DefaultUserAgent;

    /// <inheritdoc />
    /// <remarks>
    /// Default is <see cref="JsonRpcSerializerOptions.SnakeCase" />
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public virtual JsonSerializerOptions DataJsonSerializerOptions => JsonRpcSerializerOptions.SnakeCase;

    /// <inheritdoc />
    /// <remarks>
    /// Default is <see cref="JsonRpcSerializerOptions.Headers" />
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public virtual JsonSerializerOptions HeadersJsonSerializerOptions => JsonRpcSerializerOptions.Headers;

    /// <summary>
    /// Encoding used to send http request
    /// </summary>
    /// <remarks>
    /// Default is <see cref="System.Text.Encoding.UTF8" />
    /// </remarks>
    [ExcludeFromCodeCoverage]
    protected internal virtual Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// <see cref="HttpClient" /> used to send HTTP requests
    /// </summary>
    protected internal HttpClient Client { get; }

    /// <summary>
    /// Logger provided in constructor
    /// </summary>
    protected ILogger Log { get; }

    /// <summary>
    /// Service to generate id for requests
    /// </summary>
    protected IJsonRpcIdGenerator RpcIdGenerator { get; }

    /// <summary></summary>
    protected internal JsonRpcClientBase(HttpClient client, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log)
    {
        Client = client;
        RpcIdGenerator = jsonRpcIdGenerator;
        Log = log;
        InitializeClient(client);
    }

    /// <inheritdoc />
    public virtual async Task SendNotification<TParams>(string requestUrl, Notification<TParams> notification, CancellationToken cancellationToken)
        where TParams : class =>
        await SendNotificationInternal(requestUrl, notification, cancellationToken);

    /// <inheritdoc />
    public async Task SendNotification<TParams>(Notification<TParams> notification, CancellationToken cancellationToken)
        where TParams : class =>
        await SendNotificationInternal(null, notification, cancellationToken);

    /// <inheritdoc />
    public async Task SendNotification<TParams>(string requestUrl, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
    {
        var notification = new Notification<TParams>(method, parameters);
        await SendNotificationInternal(requestUrl, notification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendNotification<TParams>(string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
    {
        var notification = new Notification<TParams>(method, parameters);
        await SendNotificationInternal(null, notification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<TParams>(string requestUrl, Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class =>
        await SendRequestInternal(requestUrl, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<TParams>(Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class =>
        await SendRequestInternal(null, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<TParams>(string requestUrl, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<TParams>(string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<TParams>(string requestUrl, IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
    {
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest<TParams>(IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
    {
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string requestUrl, Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class =>
        await SendRequestInternal<TParams, TResponse>(requestUrl, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class =>
        await SendRequestInternal<TParams, TResponse>(null, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string requestUrl, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal<TParams, TResponse>(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal<TParams, TResponse>(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(string requestUrl, IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class
    {
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal<TParams, TResponse>(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TParams, TResponse>(IRpcId id, string method, TParams? parameters, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class
    {
        var request = new Request<TParams>(id, method, parameters);
        return await SendRequestInternal<TParams, TResponse>(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest(string requestUrl, Request request, CancellationToken cancellationToken)
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        return await SendRequestInternal(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest(Request request, CancellationToken cancellationToken)
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        return await SendRequestInternal(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest(string requestUrl, string method, CancellationToken cancellationToken)
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request(id, method);
        return await SendRequestInternal(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest(string method, CancellationToken cancellationToken)
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request(id, method);
        return await SendRequestInternal(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest(string requestUrl, IRpcId id, string method, CancellationToken cancellationToken)
    {
        var request = new Request(id, method);
        return await SendRequestInternal(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult> SendRequest(IRpcId id, string method, CancellationToken cancellationToken)
    {
        var request = new Request(id, method);
        return await SendRequestInternal(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string requestUrl, Request request, CancellationToken cancellationToken)
        where TResponse : class
        => await SendRequestInternal<TResponse>(requestUrl, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(Request request, CancellationToken cancellationToken)
        where TResponse : class
        => await SendRequestInternal<TResponse>(null, request, cancellationToken);

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string requestUrl, string method, CancellationToken cancellationToken)
        where TResponse : class
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request(id, method);
        return await SendRequestInternal<TResponse>(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string method, CancellationToken cancellationToken)
        where TResponse : class
    {
        var id = RpcIdGenerator.GenerateId();
        Log.LogTrace("Generated request id [{requestId}]", id);
        var request = new Request(id, method);
        return await SendRequestInternal<TResponse>(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(string requestUrl, IRpcId id, string method, CancellationToken cancellationToken)
        where TResponse : class
    {
        var request = new Request(id, method);
        return await SendRequestInternal<TResponse>(requestUrl, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ISingleJsonRpcResult<TResponse>> SendRequest<TResponse>(IRpcId id, string method, CancellationToken cancellationToken)
        where TResponse : class
    {
        var request = new Request(id, method);
        return await SendRequestInternal<TResponse>(null, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IBatchJsonRpcResult?> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal(requestUrl, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<IBatchJsonRpcResult?> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal(null, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<IBatchJsonRpcResult<TResponse>?> SendBatch<TResponse>(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal<TResponse>(requestUrl, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<IBatchJsonRpcResult<TResponse>?> SendBatch<TResponse>(IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal<TResponse>(null, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> Send(string requestUrl, ICall call, CancellationToken cancellationToken) =>
        await SendInternal(requestUrl, call, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> Send(ICall call, CancellationToken cancellationToken) =>
        await SendInternal(null, call, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> Send(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendInternal(requestUrl, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> Send(IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendInternal(null, calls, cancellationToken);

    // internal virtual for mocking in tests
    internal virtual async Task SendNotificationInternal<TParams>(string? requestUrl, Notification<TParams> notification, CancellationToken cancellationToken)
        where TParams : class
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = notification.WithSerializedParams(DataJsonSerializerOptions);
        context.WithSingle(data);
        using var content = CreateHttpContent(data);
        using var requestMessage = CreateRequestMessage(requestUrl, content, new[] { notification.Method });
        var httpResponseMessage = await Client.SendAsync(requestMessage, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
    }

    // internal virtual for mocking in tests
    internal virtual async Task<ISingleJsonRpcResult> SendRequestInternal<TParams>(string? requestUrl, Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class
    {
        var (context, contentString) = await PrepareInternalRequestContext(requestUrl, request, cancellationToken);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case SingleResponseWrapper singleResponseWrapper:
                context.WithSingleResponse(singleResponseWrapper.Response);
                Log.LogTrace("Request id [{requestId}]: success", request.Id);
                return new SingleJsonRpcResult(context, HeadersJsonSerializerOptions, DataJsonSerializerOptions);
            default:
                var message = $"Expected single response, got [{responseWrapper}]";
                Log.LogTrace("Request id [{requestId}] failed: {errorMessage}", request.Id, message);
                throw new JsonRpcException(message, context);
        }
    }

    // internal virtual for mocking in tests
    internal virtual async Task<ISingleJsonRpcResult> SendRequestInternal(string? requestUrl, Request request, CancellationToken cancellationToken)
    {
        var (context, contentString) = await PrepareInternalRequestContext(requestUrl, request, cancellationToken);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case SingleResponseWrapper singleResponseWrapper:
                context.WithSingleResponse(singleResponseWrapper.Response);
                Log.LogTrace("Request id [{requestId}]: success", request.Id);
                return new SingleJsonRpcResult(context, HeadersJsonSerializerOptions, DataJsonSerializerOptions);
            default:
                var message = $"Expected single response, got [{responseWrapper}]";
                Log.LogTrace("Request id [{requestId}] failed: {errorMessage}", request.Id, message);
                throw new JsonRpcException(message, context);
        }
    }

    // internal virtual for mocking in tests
    internal virtual async Task<ISingleJsonRpcResult<TResponse>> SendRequestInternal<TParams, TResponse>(string? requestUrl, Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class
        where TResponse : class
    {
        var (context, contentString) = await PrepareInternalRequestContext(requestUrl, request, cancellationToken);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case SingleResponseWrapper singleResponseWrapper:
                context.WithSingleResponse(singleResponseWrapper.Response);
                Log.LogTrace("Request id [{requestId}]: success", request.Id);
                return new SingleJsonRpcResult<TResponse>(context, HeadersJsonSerializerOptions, DataJsonSerializerOptions);
            default:
                var message = $"Expected single response, got [{responseWrapper}]";
                Log.LogTrace("Request id [{requestId}] failed: {errorMessage}", request.Id, message);
                throw new JsonRpcException(message, context);
        }
    }

    // internal virtual for mocking in tests
    internal virtual async Task<ISingleJsonRpcResult<TResponse>> SendRequestInternal<TResponse>(string? requestUrl, Request request, CancellationToken cancellationToken)
        where TResponse : class
    {
        var (context, contentString) = await PrepareInternalRequestContext(requestUrl, request, cancellationToken);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case SingleResponseWrapper singleResponseWrapper:
                context.WithSingleResponse(singleResponseWrapper.Response);
                Log.LogTrace("Request id [{requestId}]: success", request.Id);
                return new SingleJsonRpcResult<TResponse>(context,
                    HeadersJsonSerializerOptions,
                    DataJsonSerializerOptions);
            default:
                var message = $"Expected single response, got [{responseWrapper}]";
                Log.LogTrace("Request id [{requestId}] failed: {errorMessage}", request.Id, message);
                throw new JsonRpcException(message, context);
        }
    }

    // internal virtual for mocking in tests
    internal virtual async Task<IBatchJsonRpcResult?> SendBatchInternal(string? requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = calls.Select(x => x.WithSerializedParams(DataJsonSerializerOptions)).ToArray();
        context.WithBatch(data);
        using var content = CreateHttpContent(data);
        var methodNames = data.Select(static x => x.Method).ToArray();
        using var request = CreateRequestMessage(requestUrl, content, methodNames);
        var httpResponseMessage = await Client.SendAsync(request, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
        if (context.ExpectedBatchResponseCount == 0)
        {
            // from specification:
            // "If there are no Response objects contained within the Response array as it is to be sent to the client,
            // the server MUST NOT return an empty Array and should return nothing at all."
            Log.LogTrace("Batch count [{batchCount}] success: no response expected", data.Length);
            return null;
        }

        var contentString = await GetContent(httpResponseMessage.Content, cancellationToken);
        context.WithHttpContent(httpResponseMessage.Content, contentString);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case BatchResponseWrapper batchResponseWrapper:
                context.WithBatchResponse(batchResponseWrapper.Responses);
                Log.LogTrace("Batch count [{batchCount}] success: response count [{responseCount}]", data.Length, batchResponseWrapper.Responses.Count);
                return new BatchJsonRpcResult(context, HeadersJsonSerializerOptions, DataJsonSerializerOptions);
            case SingleResponseWrapper singleResponseWrapper:
                // "If the batch rpc call itself fails to be recognized as an valid JSON or as an Array with at least one value,
                // the response from the Server MUST be a single Response object."
                context.WithSingleResponse(singleResponseWrapper.Response);
                var message1 = $"Expected batch response, got single, id [{singleResponseWrapper.Response.Id}]";
                Log.LogTrace("Batch count [{batchCount}] failed: {errorMessage}", data.Length, message1);
                throw new JsonRpcException(message1, context);
            default:
                var message2 = $"Expected batch response, got [{responseWrapper?.GetType().Name}]";
                Log.LogTrace("Batch count [{batchCount}] failed: {errorMessage}", data.Length, message2);
                throw new JsonRpcException(message2, context);
        }
    }

    // internal virtual for mocking in tests
    internal virtual async Task<IBatchJsonRpcResult<TResponse>?> SendBatchInternal<TResponse>(string? requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = calls.Select(x => x.WithSerializedParams(DataJsonSerializerOptions)).ToArray();
        context.WithBatch(data);
        using var content = CreateHttpContent(data);
        var methodNames = data.Select(static x => x.Method).ToArray();
        using var request = CreateRequestMessage(requestUrl, content, methodNames);
        var httpResponseMessage = await Client.SendAsync(request, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
        if (context.ExpectedBatchResponseCount == 0)
        {
            // from specification:
            // "If there are no Response objects contained within the Response array as it is to be sent to the client,
            // the server MUST NOT return an empty Array and should return nothing at all."
            Log.LogTrace("Batch count [{batchCount}] success: no response expected", data.Length);
            return null;
        }

        var contentString = await GetContent(httpResponseMessage.Content, cancellationToken);
        context.WithHttpContent(httpResponseMessage.Content, contentString);
        var responseWrapper = ParseBody(contentString);
        switch (responseWrapper)
        {
            case BatchResponseWrapper batchResponseWrapper:
                context.WithBatchResponse(batchResponseWrapper.Responses);
                Log.LogTrace("Batch count [{batchCount}] success: response count [{responseCount}]", data.Length, batchResponseWrapper.Responses.Count);
                return new BatchJsonRpcResult<TResponse>(context, HeadersJsonSerializerOptions, DataJsonSerializerOptions);
            case SingleResponseWrapper singleResponseWrapper:
                // "If the batch rpc call itself fails to be recognized as an valid JSON or as an Array with at least one value,
                // the response from the Server MUST be a single Response object."
                context.WithSingleResponse(singleResponseWrapper.Response);
                var message1 = $"Expected batch response, got single, id [{singleResponseWrapper.Response.Id}]";
                Log.LogTrace("Batch count [{batchCount}] failed: {errorMessage}", data.Length, message1);
                throw new JsonRpcException(message1, context);
            default:
                var message2 = $"Expected batch response, got [{responseWrapper?.GetType().Name}]";
                Log.LogTrace("Batch count [{batchCount}] failed: {errorMessage}", data.Length, message2);
                throw new JsonRpcException(message2, context);
        }
    }

    // internal virtual for mocking in tests
    internal virtual async Task<HttpResponseMessage> SendInternal(string? requestUrl, ICall call, CancellationToken cancellationToken)
    {
        var data = call.WithSerializedParams(DataJsonSerializerOptions);
        using var content = CreateHttpContent(data);
        using var httpRequestMessage = CreateRequestMessage(requestUrl, content, new[] { call.Method });
        return await Client.SendAsync(httpRequestMessage, cancellationToken);
    }

    // internal virtual for mocking in tests
    internal virtual async Task<HttpResponseMessage> SendInternal(string? requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        var data = calls.Select(x => x.WithSerializedParams(DataJsonSerializerOptions)).ToArray();
        using var content = CreateHttpContent(data);
        var methodNames = data.Select(static x => x.Method).ToArray();
        using var message = CreateRequestMessage(requestUrl, content, methodNames);
        return await Client.SendAsync(message, cancellationToken);
    }

    // internal virtual for mocking in tests
    [ExcludeFromCodeCoverage]
    internal virtual IJsonRpcCallContext CreateContext() => new JsonRpcCallContext();

    /// <summary>
    /// Parse single or batch response from http content string
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected internal virtual IResponseWrapper? ParseBody(string contentString) =>
        JsonSerializer.Deserialize<IResponseWrapper>(contentString, HeadersJsonSerializerOptions);

    /// <summary>
    /// Serialize data to JSON body, set Encoding and Content-Type
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected internal virtual HttpContent CreateHttpContent(object data)
    {
        var body = JsonSerializer.Serialize(data, HeadersJsonSerializerOptions);
        return new StringContent(body, Encoding, JsonRpcConstants.ContentType);
    }

    /// <summary>
    /// Read content of HTTP response
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected internal virtual async Task<string> GetContent(HttpContent content, CancellationToken cancellationToken) =>
        await content.ReadAsStringAsync(cancellationToken);

    private async Task<(IJsonRpcCallContext, string)> PrepareInternalRequestContext(string? requestUrl, ICall request, CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = request.WithSerializedParams(DataJsonSerializerOptions);
        context.WithSingle(data);
        using var content = CreateHttpContent(data);
        using var requestMessage = CreateRequestMessage(requestUrl, content, new[] { request.Method });
        var httpResponseMessage = await Client.SendAsync(requestMessage, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
        var contentString = await GetContent(httpResponseMessage.Content, cancellationToken);
        context.WithHttpContent(httpResponseMessage.Content, contentString);
        return (context, contentString);
    }

    /// <summary>
    /// Set User-Agent header
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void InitializeClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        Log.LogTrace("Client initialized: user-agent {userAgent}", client.DefaultRequestHeaders.UserAgent);
    }

    private static HttpRequestMessage CreateRequestMessage(string? requestUrl, HttpContent content, string[] methodNames)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = content
        };

        httpRequestMessage.Options.Set(new HttpRequestOptionsKey<string[]>(JsonRpcConstants.OutgoingHttpRequestOptionMethodNameKey), methodNames);
        return httpRequestMessage;
    }

    private static readonly string DefaultUserAgent = typeof(JsonRpcClientBase).Namespace!;
}
