using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Features;

namespace Tochka.JsonRpc.Server.Extensions;

/// <summary>
/// Extensions for <see cref="HttpContext" /> to work with JSON Rpc request/response objects in pipeline
/// </summary>
[PublicAPI]
public static class HttpContextExtensions
{
    /// <summary>
    /// Get JSON-RPC <see cref="ICall" /> from <see cref="HttpContext.Features" />
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext" /> to get call from</param>
    /// <returns><see cref="ICall" /> if it's set, null otherwise</returns>
    public static ICall? GetJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.Call;

    /// <summary>
    /// Get raw JSON-RPC call from <see cref="HttpContext.Features" />
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext" /> to get raw call from</param>
    /// <returns><see cref="JsonDocument" /> if it's set, null otherwise</returns>
    public static JsonDocument? GetRawJsonRpcCall(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.RawCall;

    /// <summary>
    /// Get JSON-RPC <see cref="IResponse" /> from <see cref="HttpContext.Features" />
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext" /> to get response from</param>
    /// <returns><see cref="IResponse" /> if it's set, null otherwise</returns>
    public static IResponse? GetJsonRpcResponse(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.Response;

    /// <summary>
    /// Check if this call is part of batch request
    /// </summary>
    /// <param name="httpContext"><see cref="HttpContext" /> to get batch info from</param>
    public static bool JsonRpcRequestIsBatch(this HttpContext httpContext) =>
        httpContext.Features.Get<IJsonRpcFeature>()?.IsBatch ?? false;

    /// <summary>
    /// Manually set JSON-RPC response in <see cref="HttpContext.Features" />
    /// </summary>
    /// <param name="httpContext">see cref="HttpContext" /> to set response in</param>
    /// <param name="response"><see cref="IResponse" /> to set</param>
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
}
