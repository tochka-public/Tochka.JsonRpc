using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;

namespace Tochka.JsonRpc.Server;

internal static class HttpContextExtensions
{
    private const string JsonRpcId = "JsonRpcId";
    private const string JsonRpcCall = "JsonRpcId";

    public static void AddJsonRpcCall(this HttpContext httpContext, ICall call) => httpContext.Items[JsonRpcCall] = call;

    public static ICall? GetJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Items.TryGetValue(JsonRpcCall, out var call)
            ? (ICall) call!
            : null;

    public static void AddJsonRpcId(this HttpContext httpContext, IRpcId id) => httpContext.Items[JsonRpcId] = id;

    public static IRpcId GetJsonRpcId(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(JsonRpcId, out var id))
        {
            return (IRpcId) id!;
        }

        return new NullRpcId();
    }
}
