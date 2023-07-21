using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Server.Models.Response
{
    public class JsonServerResponseWrapper : IServerResponseWrapper
    {
        public IHeaderDictionary Headers { get; }
        public JToken Value { get; }

        public JsonServerResponseWrapper(JToken value, IUntypedCall call, IHeaderDictionary headers)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = SetId(value, call);
            Headers = headers;
        }

        internal static JToken SetId(JToken value, IUntypedCall call)
        {
            if (call is UntypedRequest request)
            {
                value[JsonRpcConstants.IdProperty] = request.RawId;
            }

            return value;
        }

        /// <summary>
        /// Set code 200, set content-type, add headers if not null, write json body if not null
        /// </summary>
        /// <returns></returns>
        public async Task Write(HandlingContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer)
        {
            var sink = context.OriginalHttpContext.Response;
            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    sink.Headers.Append(header.Key, header.Value);
                }
            }

            sink.StatusCode = 200;
            sink.ContentLength = null;
            var responseMediaType = new MediaTypeHeaderValue(JsonRpcConstants.ContentType)
            {
                Encoding = context.RequestEncoding
            };
            sink.ContentType = responseMediaType.ToString();

            if (!context.WriteResponse)
            {
                return;
            }

            await using (var writer = new HttpResponseStreamWriter(sink.Body, context.RequestEncoding))
            {
                using var jsonWriter = new JsonTextWriter(writer)
                {
                    CloseOutput = false,
                    AutoCompleteOnClose = false,
                    Formatting = headerJsonRpcSerializer.Settings.Formatting
                };
                await Value.WriteToAsync(jsonWriter, context.OriginalHttpContext.RequestAborted);
            }

            SetResponseContextItem(context.OriginalHttpContext);
        }

        /// <summary>
        /// Store ambient information about response. Useful for metrics
        /// </summary>
        /// <param name="context"></param>
        private void SetResponseContextItem(HttpContext context)
        {
            var errorCode = (Value as JObject)?[JsonRpcConstants.ErrorProperty]?[JsonRpcConstants.ErrorCodeProperty]?.Value<string>();
            if (!string.IsNullOrEmpty(errorCode))
            {
                context.Items[JsonRpcConstants.ResponseErrorCodeItemKey] = errorCode;
            }
        }
    }
}
