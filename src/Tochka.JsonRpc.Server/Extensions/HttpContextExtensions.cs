using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Features;

namespace Tochka.JsonRpc.Server.Extensions;

public static class HttpContextExtensions
{
    public static ICall? GetJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.Call;

    public static JsonDocument? GetRawJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.RawCall;

    public static IResponse? GetJsonRpcResponse(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.Response;

    public static bool JsonRpcRequestIsBatch(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.IsBatch ?? false;

    [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod", Justification = "Need to register it as interface to use it as key")]
    public static void SetJsonRpcResponse(this HttpContext httpContext, IResponse response)
    {
        var feature = httpContext.Features.Get<IJsonRpcFeature>();
        if (feature == null)
        {
            feature = new JsonRpcFeature();
            httpContext.Features.Set<IJsonRpcFeature>(feature);
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
