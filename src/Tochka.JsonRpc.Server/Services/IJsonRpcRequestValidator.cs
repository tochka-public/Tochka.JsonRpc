using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Tochka.JsonRpc.Server.Services;

/// <summary>
/// Service to validate JSON-RPC requests
/// </summary>
[PublicAPI]
public interface IJsonRpcRequestValidator
{
    /// <summary>
    /// Check if HttpRequest should be processed by JSON-RPC pipeline
    /// </summary>
    /// <param name="httpContext">Request <see cref="HttpContext" /></param>
    bool IsJsonRpcRequest(HttpContext httpContext);
}
