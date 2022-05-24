using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Exceptions;

namespace Tochka.JsonRpc.Server.Models.Response
{
    public class RawServerResponseWrapper : IServerResponseWrapper
    {
        public HttpResponse Source { get; }

        public RawServerResponseWrapper(HttpResponse source)
        {
            Source = source;
        }

        /// <summary>
        /// Copy code, headers and body from nested response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="headerJsonRpcSerializer"></param>
        /// <returns></returns>
        public async Task Write(HandlingContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer)
        {
            var sink = context.OriginalHttpContext.Response;
            sink.StatusCode = Source.StatusCode;
            sink.ContentLength = null;

            if (Source.Headers != null)
            {
                foreach (var header in Source.Headers)
                {
                    sink.Headers.Append(header.Key, header.Value);
                }
            }

            // TODO is this sane? Maybe return body since we're already violating protocol?
            if (!context.WriteResponse)
            {
                return;
            }

            if (Source.Body is not MemoryStream ms)
            {
                throw new JsonRpcInternalException("Expected MemoryStream to read response");
            }

            ms.Seek(0, SeekOrigin.Begin);
            await ms.CopyToAsync(sink.Body, 81920, context.OriginalHttpContext.RequestAborted);
        }
    }
}