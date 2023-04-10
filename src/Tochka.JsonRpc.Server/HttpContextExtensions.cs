using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;

namespace Tochka.JsonRpc.Server;

internal static class HttpContextExtensions
{
    private const string JsonRpcCall = "JsonRpcCall";

    public static void SetJsonRpcCall(this HttpContext httpContext, ICall call) => httpContext.Items[JsonRpcCall] = call;

    public static ICall? GetJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Items.TryGetValue(JsonRpcCall, out var call)
            ? (ICall) call!
            : null;

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
