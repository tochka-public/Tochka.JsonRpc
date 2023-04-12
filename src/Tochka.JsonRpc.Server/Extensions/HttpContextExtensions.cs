using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Server.Extensions;

internal static class HttpContextExtensions
{
    private const string JsonRpcCall = "JsonRpcCall";
    private const string JsonRpcResponse = "JsonRpcResponse";
    private const string JsonRpcIsBatch = "JsonRpcIsBatch";

    public static void SetJsonRpcCall(this HttpContext httpContext, ICall call) => httpContext.Items[JsonRpcCall] = call;

    public static ICall? GetJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Items.TryGetValue(JsonRpcCall, out var call)
            ? (ICall) call!
            : null;

    public static void SetJsonRpcResponse(this HttpContext httpContext, IResponse response) => httpContext.Items[JsonRpcResponse] = response;

    public static IResponse? GetJsonRpcResponse(this HttpContext httpContext) =>
        httpContext.Items.TryGetValue(JsonRpcResponse, out var response)
            ? (IResponse) response!
            : null;

    public static void SetJsonRpcRequestIsBatch(this HttpContext httpContext, IResponse response) => httpContext.Items[JsonRpcIsBatch] = response;

    public static bool JsonRpcRequestIsBatch(this HttpContext httpContext) =>
        httpContext.Items.TryGetValue(JsonRpcIsBatch, out var isBatch) && (bool) isBatch!;

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
