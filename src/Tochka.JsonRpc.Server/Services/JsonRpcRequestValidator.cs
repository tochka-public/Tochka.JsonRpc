using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services;

/// <inheritdoc />
internal partial class JsonRpcRequestValidator : IJsonRpcRequestValidator
{
    private readonly TemplateMatcher templateMatcher;

    public JsonRpcRequestValidator(IOptions<JsonRpcServerOptions> options)
    {
        var routePrefix = options.Value.RoutePrefix.Value;
        if (string.IsNullOrEmpty(routePrefix))
        {
            throw new ArgumentNullException(nameof(options.Value.RoutePrefix));
        }

        var routeTemplate = BuildRouteTemplate(routePrefix);
        templateMatcher = new TemplateMatcher(TemplateParser.Parse(routeTemplate), new RouteValueDictionary());
    }

    public bool IsJsonRpcRequest(HttpContext httpContext)
    {
        if (!templateMatcher.TryMatch(httpContext.Request.Path, new RouteValueDictionary()))
        {
            return false;
        }

        if (httpContext.Request.Method != HttpMethods.Post)
        {
            return false;
        }

        var contentType = httpContext.Request.GetTypedHeaders().ContentType;
        if (contentType == null)
        {
            return false;
        }

        if (contentType.MediaType.Value is not { } mediaTypeHeaderValue || !JsonRpcConstants.AllowedRequestContentType.Contains(mediaTypeHeaderValue))
        {
            return false;
        }
        return true;
    }

    // internal for tests
    internal static string BuildRouteTemplate(string routePrefix)
    {
        // A catch-all parameter can only appear as the last segment of the route template
        // If it's already in prefix, no need to add it, it will match any route anyway
        var routeTemplate = RouteWithWildcardSuffixRegex.IsMatch(routePrefix)
            ? routePrefix
            : $"{routePrefix.TrimEnd('/')}/{{*suffix}}";

        // TemplateMatcher work only with { }, but [ ] for controller, action and area required to be replaced in autodoc
        return routeTemplate
            .Replace("[controller]", "{controller}")
            .Replace("[action]", "{action}")
            .Replace("[area]", "{area}");
    }

    [GeneratedRegex(@"^.*?\{\*.*?}/?$")]
    private static partial Regex RouteWithWildcardSuffix();

    // internal for tests
    internal static readonly Regex RouteWithWildcardSuffixRegex = RouteWithWildcardSuffix(); // "smth/{*smth}" or "smth/{*smth}/"
}
