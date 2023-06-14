using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.OpenRpc.Services;

namespace Tochka.JsonRpc.OpenRpc;

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

    public async Task InvokeAsync(HttpContext httpContext, IOpenRpcDocumentGenerator documentGenerator)
    {
        var documentName = GetDocumentName(httpContext.Request);
        if (string.IsNullOrEmpty(documentName))
        {
            await next(httpContext);
            return;
        }

        if (!options.Docs.TryGetValue(documentName, out var info))
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var host = new UriBuilder(httpContext.Request.GetEncodedUrl())
        {
            Path = null,
            Fragment = null,
            Query = null
        };
        var document = documentGenerator.Generate(info, documentName, host.Uri);
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = "application/json;charset=utf-8";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, document, OpenRpcConstants.JsonSerializerOptions);
    }

    private string? GetDocumentName(HttpRequest request)
    {
        if (request.Method != HttpMethods.Get)
        {
            return null;
        }

        var routeValues = new RouteValueDictionary();
        if (!requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.TryGetValue(OpenRpcConstants.DocumentTemplateParameterName, out var document))
        {
            return null;
        }

        return document!.ToString();
    }
}
