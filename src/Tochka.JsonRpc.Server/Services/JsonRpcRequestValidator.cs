using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services;

/// <inheritdoc />
internal class JsonRpcRequestValidator
(
    IOptions<JsonRpcServerOptions> options
) : IJsonRpcRequestValidator
{
    public bool IsJsonRpcRequest(HttpContext httpContext)
    {
        if (httpContext.Request.Method != HttpMethods.Post)
        {
            return false;
        }

        var contentType = httpContext.Request.GetTypedHeaders().ContentType;
        if (contentType?.MediaType.Value is not { } mediaTypeHeaderValue || !JsonRpcConstants.AllowedRequestContentType.Contains(mediaTypeHeaderValue))
        {
            return false;
        }

        if (httpContext.GetJsonRpcResponseMediaType() is null)
        {
            return false;
        }

        var markerSegment = options.Value.UrlMarkerSegment;
        if (string.IsNullOrWhiteSpace(markerSegment) || markerSegment.Contains('/'))
        {
            throw new ArgumentException($"Invalid JsonRpcServerOptions.UrlMarkerSegment [{markerSegment}]: empty or contains '/'");
        }

        return httpContext.Request.Path.Value?.Split('/').Any(x => x.Equals(markerSegment, StringComparison.OrdinalIgnoreCase)) == true;
    }
}
