using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Client.Models;
using Tochka.JsonRpc.V1.Client.Services;
using Tochka.JsonRpc.V1.Client.Settings;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Models.Id;
using Tochka.JsonRpc.V1.Common.Models.Request;
using Tochka.JsonRpc.V1.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Client
{
    /// <summary>
    /// Base class for JSON Rpc clients
    /// </summary>
    public abstract class JsonRpcClientBase : IJsonRpcClient
    {
        protected readonly HeaderJsonRpcSerializer HeaderJsonRpcSerializer;

        protected readonly ILogger log;

        protected readonly HttpClient Client;

        protected readonly JsonRpcClientOptionsBase Options;

        protected readonly IJsonRpcIdGenerator JsonRpcIdGenerator;

        protected internal virtual Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Default is "Tochka.JsonRpc.V1.Client"
        /// </summary>
        public virtual string UserAgent => typeof(JsonRpcClientBase).Namespace;

        protected internal JsonRpcClientBase(HttpClient client, IJsonRpcSerializer serializer, HeaderJsonRpcSerializer headerJsonRpcSerializer, JsonRpcClientOptionsBase options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger log)
        {
            Client = client;
            Serializer = serializer;
            HeaderJsonRpcSerializer = headerJsonRpcSerializer;
            Options = options;
            this.JsonRpcIdGenerator = jsonRpcIdGenerator;
            this.log = log;
            InitializeClient(client, options);
        }

        /// <inheritdoc />
        public virtual async Task SendNotification<T>(string requestUrl, Notification<T> notification, CancellationToken cancellationToken)
        {
            var context = CreateContext();
            context.WithRequestUrl(requestUrl);
            var data = notification.WithSerializedParams(Serializer);
            if (Options.LogRequests)
            {
                log.LogInformation("{jsonRpcPayload}", data);
            }
            context.WithSingle(data);
            var content = CreateHttpContent(data);
            var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
            context.WithHttpResponse(httpResponseMessage);
        }

        /// <inheritdoc />
        public virtual async Task SendNotification<T>(Notification<T> notification, CancellationToken cancellationToken)
        {
            await SendNotification(null, notification, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task SendNotification<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
        {
            var notification = new Notification<T>
            {
                Method = method,
                Params = parameters
            };
            await SendNotification(requestUrl, notification, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task SendNotification<T>(string method, T parameters, CancellationToken cancellationToken)
        {
            var notification = new Notification<T>
            {
                Method = method,
                Params = parameters
            };
            await SendNotification(null, notification, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, Request<T> request, CancellationToken cancellationToken)
        {
            var context = CreateContext();
            context.WithRequestUrl(requestUrl);
            var data = request.WithSerializedParams(Serializer);
            if (Options.LogRequests)
            {
                log.LogInformation("{jsonRpcPayload}", data);
            }
            context.WithSingle(data);
            var content = CreateHttpContent(data);
            var httpResponseMessage = await Client.PostAsync((string) null, content, cancellationToken);
            context.WithHttpResponse(httpResponseMessage);
            var contentString = await GetContent(httpResponseMessage.Content);
            context.WithHttpContent(httpResponseMessage.Content, contentString);
            var responseWrapper = ParseBody(contentString);
            switch (responseWrapper)
            {
                case SingleResponseWrapper singleResponseWrapper:
                    context.WithSingleResponse(singleResponseWrapper.Single);
                    log.LogTrace($"Request id [{request.Id}]: success");
                    return new SingleJsonRpcResult(context, HeaderJsonRpcSerializer, Serializer);
                default:
                    var message = $"Expected single response, got [{responseWrapper}]";
                    log.LogTrace($"Request id [{request.Id}] failed: {message}");
                    throw new JsonRpcException(message, context);
            }
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(Request<T> request, CancellationToken cancellationToken)
        {
            return await SendRequest(null, request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, string method, T parameters, CancellationToken cancellationToken)
        {
            var id = JsonRpcIdGenerator.GenerateId();
            var request = new Request<T>
            {
                Id = id,
                Method = method,
                Params = parameters
            };
            return await SendRequest(requestUrl, request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string method, T parameters, CancellationToken cancellationToken)
        {
            return await SendRequest((string) null, method, parameters, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(string requestUrl, IRpcId id, string method, T parameters, CancellationToken cancellationToken)
        {
            var request = new Request<T>
            {
                Id = id,
                Method = method,
                Params = parameters
            };
            return await SendRequest(requestUrl, request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ISingleJsonRpcResult> SendRequest<T>(IRpcId id, string method, T parameters, CancellationToken cancellationToken)
        {
            return await SendRequest(null, id, method, parameters, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IBatchJsonRpcResult> SendBatch(string requestUrl, IEnumerable<ICall> calls, CancellationToken cancellationToken)
        {
            var context = CreateContext();
            context.WithRequestUrl(requestUrl);
            var data = calls.Select(x => x.WithSerializedParams(Serializer)).ToList();
            if (Options.LogRequests)
            {
                log.LogInformation("{jsonRpcPayload}", data);
            }
            context.WithBatch(data);
            var content = CreateHttpContent(data);
            var httpResponseMessage = await Client.PostAsync(requestUrl, content, cancellationToken);
            context.WithHttpResponse(httpResponseMessage);
            if (context.ExpectedBatchResponseCount == 0)
            {
                // "If there are no Response objects contained within the Response array as it is to be sent to the client,
                // the server MUST NOT return an empty Array and should return nothing at all."
                log.LogTrace($"Batch count [{data.Count}] success: no response expected");
                return null;
            }

            var contentString = await GetContent(httpResponseMessage.Content);
            context.WithHttpContent(httpResponseMessage.Content, contentString);
            var responseWrapper = ParseBody(contentString);
            switch (responseWrapper)
            {
                case BatchResponseWrapper batchResponseWrapper:
                    context.WithBatchResponse(batchResponseWrapper.Batch);
                    log.LogTrace($"Batch count [{data.Count}] success: response count {batchResponseWrapper.Batch.Count}");
                    return new BatchJsonRpcResult(context, HeaderJsonRpcSerializer, Serializer);
                case SingleResponseWrapper singleResponseWrapper:
                    // "If the batch rpc call itself fails to be recognized as an valid JSON or as an Array with at least one value,
                    // the response from the Server MUST be a single Response object."
                    context.WithSingleResponse(singleResponseWrapper.Single);
                    var message1 = $"Expected batch response, got single, id [{singleResponseWrapper.Single.Id}]";
                    log.LogTrace($"Batch count [{data.Count}] failed: {message1}");
                    throw new JsonRpcException(message1, context);
                default:
                    var message2 = $"Expected batch response, got [{responseWrapper?.GetType().Name}]";
                    log.LogTrace($"Batch count [{data.Count}] failed: {message2}");
                    throw new JsonRpcException(message2, context);
            }
        }

        /// <inheritdoc />
        public virtual async Task<IBatchJsonRpcResult> SendBatch(IEnumerable<ICall> calls, CancellationToken cancellationToken)
        {
            return await SendBatch(null, calls, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<HttpResponseMessage> Send(string requestUrl, ICall call, CancellationToken cancellationToken)
        {
            var data = call.WithSerializedParams(Serializer);
            var content = CreateHttpContent(data);
            return await Client.PostAsync(requestUrl, content, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<HttpResponseMessage> Send(ICall call, CancellationToken cancellationToken)
        {
            return await Send(null, call, cancellationToken);
        }

        /// <inheritdoc />
        public IJsonRpcSerializer Serializer { get; }

        /// <summary>
        /// Set client properties from base options
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        protected internal virtual void InitializeClient(HttpClient client, JsonRpcClientOptionsBase options)
        {
            client.BaseAddress = new Uri(options.Url, UriKind.Absolute);
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.Timeout = options.Timeout;
            log.LogTrace($"Client initialized: url {client.BaseAddress}, user-agent {client.DefaultRequestHeaders.UserAgent}, timeout {client.Timeout.TotalSeconds}s.");
        }

        /// <summary>
        /// Serialize data to JSON body, set Content-Type
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected internal virtual HttpContent CreateHttpContent(object data)
        {
            var body = JsonConvert.SerializeObject(data, HeaderJsonRpcSerializer.Settings);
            return new StringContent(body, Encoding, JsonRpcConstants.ContentType);
        }

        protected internal virtual async Task<string> GetContent(HttpContent content)
        {
            if (content == null)
            {
                return null;
            }

            using (var stream = await content.ReadAsStreamAsync())
            {
                using (var streamReader = new StreamReader(stream, Encoding))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        /// <summary>
        /// Parse single or batch response from http content string
        /// </summary>
        /// <param name="contentString"></param>
        /// <returns></returns>
        protected internal virtual IResponseWrapper ParseBody(string contentString)
        {
            var json = JToken.Parse(contentString);
            return json.ToObject<IResponseWrapper>(HeaderJsonRpcSerializer.Serializer);
        }

        protected internal virtual IJsonRpcCallContext CreateContext()
        {
            return new JsonRpcCallContext();
        }
    }
}
