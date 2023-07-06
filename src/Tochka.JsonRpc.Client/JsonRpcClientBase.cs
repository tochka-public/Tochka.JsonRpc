using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Settings;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Client;

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

    protected internal HttpClient Client { get; }

    protected ILogger Log { get; }
    protected IJsonRpcIdGenerator RpcIdGenerator { get; }

    protected internal JsonRpcClientBase(HttpClient client, JsonRpcClientOptionsBase options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log)
    {
        Client = client;
        RpcIdGenerator = jsonRpcIdGenerator;
        Log = log;
        InitializeClient(client, options);
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
    public async Task<IBatchJsonRpcResult?> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal(requestUrl, calls, cancellationToken);

    /// <inheritdoc />
    public async Task<IBatchJsonRpcResult?> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken) =>
        await SendBatchInternal(null, calls, cancellationToken);

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
        var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
    }

    // internal virtual for mocking in tests
    internal virtual async Task<ISingleJsonRpcResult> SendRequestInternal<TParams>(string? requestUrl, Request<TParams> request, CancellationToken cancellationToken)
        where TParams : class
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = request.WithSerializedParams(DataJsonSerializerOptions);
        context.WithSingle(data);
        using var content = CreateHttpContent(data);
        var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
        context.WithHttpResponse(httpResponseMessage);
        var contentString = await GetContent(httpResponseMessage.Content, cancellationToken);
        context.WithHttpContent(httpResponseMessage.Content, contentString);
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
    internal virtual async Task<IBatchJsonRpcResult?> SendBatchInternal(string? requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        var context = CreateContext();
        context.WithRequestUrl(requestUrl);
        var data = calls.Select(x => x.WithSerializedParams(DataJsonSerializerOptions)).ToArray();
        context.WithBatch(data);
        using var content = CreateHttpContent(data);
        var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
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
    internal virtual async Task<HttpResponseMessage> SendInternal(string? requestUrl, ICall call, CancellationToken cancellationToken)
    {
        var data = call.WithSerializedParams(DataJsonSerializerOptions);
        using var content = CreateHttpContent(data);
        return await Client.PostAsync(requestUrl, content, cancellationToken);
    }

    // internal virtual for mocking in tests
    internal virtual async Task<HttpResponseMessage> SendInternal(string? requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
    {
        var data = calls.Select(x => x.WithSerializedParams(DataJsonSerializerOptions));
        using var content = CreateHttpContent(data);
        return await Client.PostAsync(requestUrl, content, cancellationToken);
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

    /// <summary>
    /// Set HttpClient properties from base options
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void InitializeClient(HttpClient client, JsonRpcClientOptionsBase options)
    {
        if (!options.Url.EndsWith('/'))
        {
            throw new ArgumentException("Base url should end with '/' to prevent unexpected behavior when joining url parts", nameof(options.Url));
        }

        client.BaseAddress = new Uri(options.Url, UriKind.Absolute);
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        client.Timeout = options.Timeout;
        Log.LogTrace("Client initialized: url {baseUrl}, user-agent {userAgent}, timeout {timeout}s", client.BaseAddress, client.DefaultRequestHeaders.UserAgent, client.Timeout.TotalSeconds);
    }

    private static readonly string DefaultUserAgent = typeof(JsonRpcClientBase).Namespace!;
}
