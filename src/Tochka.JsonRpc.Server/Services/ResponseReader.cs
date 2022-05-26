using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Models.Response;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services
{
    public class ResponseReader : IResponseReader
    {
        private readonly IJsonRpcErrorFactory errorFactory;
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;
        private readonly ILogger log;

        public ResponseReader(IJsonRpcErrorFactory errorFactory, HeaderJsonRpcSerializer headerJsonRpcSerializer, ILogger<ResponseReader> log)
        {
            this.errorFactory = errorFactory;
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
            this.log = log;
        }

        public virtual async Task<IServerResponseWrapper> GetResponse(HttpContext nestedHttpContext, IUntypedCall call, bool allowRawResponses, CancellationToken token)
        {
            var actionResultType = Utils.GetActionResultType(nestedHttpContext);

            log.LogTrace("{action}: {allowRawResponsesName} is [{allowRawResponses}], {actionResultTypeName} is [{actionResultType}]",
                         nameof(GetResponse),
                         nameof(allowRawResponses),
                         allowRawResponses,
                         nameof(actionResultType),
                         actionResultType);
            
            var responseWrapper = await GetResponseWrapper(nestedHttpContext.Response, actionResultType, call, token);

            return responseWrapper switch
            {
                RawServerResponseWrapper _ when !allowRawResponses => throw new JsonRpcInternalException($"Raw responses are not allowed by default and not supported in batches, check {nameof(JsonRpcOptions)}"),
                null => throw new ArgumentNullException(nameof(responseWrapper)),
                _ => responseWrapper
            };
        }

        protected internal virtual async Task<IServerResponseWrapper> GetResponseWrapper(HttpResponse response, Type actionResultType, IUntypedCall call, CancellationToken token)
        {
            if (response.Body is not MemoryStream ms)
            {
                throw new JsonRpcInternalException("Expected MemoryStream to read response");
            }

            var contentType = response.GetTypedHeaders().ContentType;
            // TODO not sure if this will be right encoding
            var encoding = contentType?.Encoding ?? Encoding.UTF8;
            log.LogTrace("{action}: encoding is [{encodingName}]", nameof(GetResponseWrapper), encoding.EncodingName);
            ms.Seek(0, SeekOrigin.Begin);
            return await HandleResultType(response, actionResultType, call, ms, encoding, token);
        }

        protected internal virtual async Task<IServerResponseWrapper> HandleResultType(HttpResponse response, Type actionResultType, IUntypedCall call, MemoryStream responseBody, Encoding encoding, CancellationToken token)
        {
            return actionResultType switch
            {
                {} objectResult when objectResult == typeof(ObjectResult) => await HandleObjectResult(response,
                                                                                                      call,
                                                                                                      responseBody,
                                                                                                      encoding,
                                                                                                      token),
                null => await HandleNullResult(response,
                                               call,
                                               responseBody,
                                               encoding,
                                               token),
                _ => await HandleUnknownResult(response,
                                               call,
                                               responseBody,
                                               encoding,
                                               token)
            };
        }

        /// <summary>
        /// Normal result of action when it returns object
        /// </summary>
        /// <param name="response"></param>
        /// <param name="call"></param>
        /// <param name="responseBody"></param>
        /// <param name="encoding"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected internal virtual async Task<IServerResponseWrapper> HandleObjectResult(HttpResponse response, IUntypedCall call, MemoryStream responseBody, Encoding encoding, CancellationToken token)
        {
            log.LogTrace("{action}", nameof(HandleObjectResult));
            var jToken = await ReadJsonResponse(responseBody, encoding, token);
            return new JsonServerResponseWrapper(jToken, call, response.Headers);
        }

        /// <summary>
        /// Framework decided to reject our request, eg. 404 when routing failed
        /// </summary>
        /// <param name="response"></param>
        /// <param name="call"></param>
        /// <param name="responseBody"></param>
        /// <param name="encoding"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected internal virtual Task<IServerResponseWrapper> HandleNullResult(HttpResponse response, IUntypedCall call, MemoryStream responseBody, Encoding encoding, CancellationToken token)
        {
            log.LogTrace("{action}", nameof(HandleNullResult));
            var body = ReadTextBody(responseBody, encoding);
            var jToken = FormatHttpErrorResponse(response, body);
            return Task.FromResult<IServerResponseWrapper>(new JsonServerResponseWrapper(jToken, call, response.Headers));
        }

        /// <summary>
        /// Unknown action results are treated as raw responses
        /// </summary>
        /// <param name="response"></param>
        /// <param name="call"></param>
        /// <param name="responseBody"></param>
        /// <param name="encoding"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected internal virtual Task<IServerResponseWrapper> HandleUnknownResult(HttpResponse response, IUntypedCall call, MemoryStream responseBody, Encoding encoding, CancellationToken token)
        {
            log.LogTrace("{action}: return raw response", nameof(GetResponseWrapper));
            return Task.FromResult<IServerResponseWrapper>(new RawServerResponseWrapper(response));
        }
        
        protected internal virtual async Task<JToken> ReadJsonResponse(MemoryStream ms, Encoding encoding, CancellationToken token)
        {
            using (var reader = new StreamReader(ms, encoding))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    // we obey how response was serialized
                    log.LogTrace("Reading json from body");
                    return await JToken.ReadFromAsync(jsonReader, token);
                }
            }
        }

        protected internal virtual string ReadTextBody(MemoryStream ms, Encoding encoding)
        {
            log.LogTrace("Reading text body");
            return ms.Length == 0
                ? null
                : encoding.GetString(ms.ToArray());
        }

        protected internal virtual JToken FormatHttpErrorResponse(HttpResponse response, string rawBody)
        {
            log.LogTrace("{action}: http code is [{responseStatusCode}]", nameof(FormatHttpErrorResponse), response.StatusCode);

            return response.StatusCode switch
            {
                404 => errorFactory.ConvertErrorToResponse(errorFactory.MethodNotFound(rawBody), headerJsonRpcSerializer),
                _ => errorFactory.ConvertErrorToResponse(errorFactory.InternalError(rawBody), headerJsonRpcSerializer)
            };
        }
    }
}