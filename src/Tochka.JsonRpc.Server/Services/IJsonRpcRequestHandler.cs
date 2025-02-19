using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Server.Services;

/// <summary>
/// Service to process JSON-RPC calls in pipeline
/// </summary>
public interface IJsonRpcRequestHandler
{
    /// <summary>
    /// Process JSON-RPC requests in pipeline
    /// </summary>
    /// <param name="requestWrapper">Incoming JSON-RPC call</param>
    /// <param name="httpContext">Request <see cref="HttpContext" /></param>
    /// <param name="next">Delegate from middleware</param>
    /// <returns>Response to provided call if it is expected, null otherwise</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "next is common name for middlewares")]
    Task<IResponseWrapper?> ProcessJsonRpcRequest(IRequestWrapper? requestWrapper, HttpContext httpContext, RequestDelegate next);
}
