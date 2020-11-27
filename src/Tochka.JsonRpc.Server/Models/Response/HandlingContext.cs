using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Tochka.JsonRpc.Server.Models.Response
{
    [ExcludeFromCodeCoverage]
    public class HandlingContext
    {
        public HandlingContext(HttpContext originalHttpContext, Encoding requestEncoding, RequestDelegate next)
        {
            OriginalHttpContext = originalHttpContext;
            RequestEncoding = requestEncoding;
            Next = next;
            WriteResponse = false;
        }

        public HttpContext OriginalHttpContext { get; }
        public Encoding RequestEncoding { get; }
        public RequestDelegate Next { get; }

        /// <summary>
        /// Set to true if response content should be writen to result HTTPResponse
        /// </summary>
        public bool WriteResponse { get; set; }
    }
}