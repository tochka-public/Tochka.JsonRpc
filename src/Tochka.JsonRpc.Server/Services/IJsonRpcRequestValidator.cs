using System.Text;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Server.Services;

/// <summary>
/// Service to validate JSON-RPC requests
/// </summary>
public interface IJsonRpcRequestValidator
{
    /// <summary>
    /// Check if HttpRequest should be processed by JSON-RPC pipeline
    /// </summary>
    /// <param name="httpContext">Request <see cref="HttpContext" /></param>
    public bool IsJsonRpcRequest(HttpContext httpContext);
}
