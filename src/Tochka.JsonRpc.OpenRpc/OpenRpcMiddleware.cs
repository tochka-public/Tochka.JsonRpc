using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Tochka.JsonRpc.OpenRpc
{
    public class OpenRpcMiddleware
    {
        private readonly RequestDelegate next;
        private readonly TemplateMatcher requestMatcher;
        private readonly OpenRpcOptions options;

        public OpenRpcMiddleware(RequestDelegate next, IOptions<OpenRpcOptions> options)
        {
            this.next = next;
            this.options = options.Value;
            requestMatcher = new TemplateMatcher(TemplateParser.Parse(options.Value.DocumentPath), new RouteValueDictionary());
        }

        public async Task Invoke(HttpContext httpContext, OpenRpcGenerator openRpcGenerator)
        {
            var documentName = GetDocumentName(httpContext.Request);
            if (!string.IsNullOrEmpty(documentName))
            {
                await ReturnDocument(httpContext, openRpcGenerator, documentName);
            }
            else
            {
                await next(httpContext);
            }
        }

        private async Task ReturnDocument(HttpContext httpContext, OpenRpcGenerator openRpcGenerator, string documentName)
        {
            // TODO test if url is correctly created in middleware
            var host = new UriBuilder(httpContext.Request.GetEncodedUrl())
            {
                Path = null,
                Fragment = null,
                Query = null
            };
            if (options.Docs.TryGetValue(documentName, out var info))
            {
                var document = openRpcGenerator.GetDocument(info, documentName, host.Uri);
                await WriteJson(httpContext.Response, document);
            }
            else
            {
                httpContext.Response.StatusCode = 404;

            }
        }

        private string GetDocumentName(HttpRequest request)
        {
            if (request.Method != HttpMethods.Get)
            {
                return null;
            }

            var routeValues = new RouteValueDictionary();
            if (!requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.ContainsKey(OpenRpcConstants.DocumentTemplateParameterName))
            {
                return null;
            }

            return routeValues[OpenRpcConstants.DocumentTemplateParameterName].ToString();
        }

        private async Task WriteJson(HttpResponse response, Models.OpenRpc document)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";
            
            var json = JsonConvert.SerializeObject(document, OpenRpcConstants.JsonSerializerSettings);
            await response.WriteAsync(json, new UTF8Encoding(false));
        }
    }
}