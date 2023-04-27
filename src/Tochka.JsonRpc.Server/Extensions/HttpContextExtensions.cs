using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Server.Extensions;

internal static class HttpContextExtensions
{
    public static ICall? GetJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Features.Get<JsonRpcFeature>()?.Call;

    public static JsonDocument? GetRawJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Features.Get<JsonRpcFeature>()?.RawCall;

    public static IResponse? GetJsonRpcResponse(this HttpContext httpContext) =>
        httpContext.Features.Get<JsonRpcFeature>()?.Response;

    public static bool JsonRpcRequestIsBatch(this HttpContext httpContext) =>
        httpContext.Features.Get<JsonRpcFeature>()?.IsBatch ?? false;

    public static void SetJsonRpcResponse(this HttpContext httpContext, IResponse response)
    {
        var feature = httpContext.Features.Get<JsonRpcFeature>();
        if (feature == null)
        {
            feature = new JsonRpcFeature();
            httpContext.Features.Set(feature);
        }

        feature.Response = response;
    }

    public static bool IsJsonRpcRequest(this HttpContext httpContext, PathString jsonRpcPrefix)
    {
        if (!httpContext.Request.Path.StartsWithSegments(jsonRpcPrefix))
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

        if (!contentType.MediaType.Equals(JsonRpcConstants.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
