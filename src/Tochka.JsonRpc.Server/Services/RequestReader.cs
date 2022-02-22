using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Exceptions;

namespace Tochka.JsonRpc.Server.Services
{
    public class RequestReader : IRequestReader
    {
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;
        private readonly ILogger log;

        public RequestReader(HeaderJsonRpcSerializer headerJsonRpcSerializer, ILogger<RequestReader> log)
        {
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
            this.log = log;
        }

        public virtual async Task<IRequestWrapper> GetRequestWrapper(HttpContext context, Encoding encoding)
        {
            // get cached value
            if (context.Items.ContainsKey(JsonRpcConstants.RequestItemKey))
            {
                var call = context.Items[JsonRpcConstants.RequestItemKey] as IUntypedCall;
                log.LogTrace($"Found cached call in HttpContext");
                return new SingleRequestWrapper()
                {
                    Call = call
                };
            }

            try
            {
                var requestWrapper = await ParseRequest(context, encoding);
                if (!(requestWrapper is SingleRequestWrapper singleRequestWrapper))
                {
                    return requestWrapper;
                }

                if (singleRequestWrapper.Call.Jsonrpc != JsonRpcConstants.Version)
                {
                    throw new JsonRpcInternalException($"Invalid JSON Rpc version [{singleRequestWrapper.Call.Jsonrpc}]");
                }

                if (string.IsNullOrEmpty(singleRequestWrapper.Call.Method))
                {
                    throw new JsonRpcInternalException($"Method is null or empty");
                }

                context.Items[JsonRpcConstants.RequestItemKey] = singleRequestWrapper.Call;
                log.LogTrace($"Caching call in HttpContext");
                return requestWrapper;
            }
            catch (Exception e)
            {
                log.LogWarning(e, $"{nameof(GetRequestWrapper)} failed, return bad request");
                return new BadRequestWrapper
                {
                    Exception = e
                };
            }
        }

        internal virtual async Task<IRequestWrapper> ParseRequest(HttpContext context, Encoding encoding)
        {
            context.Request.EnableBuffering();
            var body = context.Request.Body;

            using (var streamReader = new HttpRequestStreamReader(body, encoding))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    log.LogTrace($"Reading request body");
                    var json = await JToken.ReadFromAsync(jsonReader, context.RequestAborted);
                    var result = json.ToObject<IRequestWrapper>(headerJsonRpcSerializer.Serializer);
                    body.Position = 0;
                    return result;
                }
            }
        }
    }
}