using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Server.Services;

public interface IJsonRpcRequestHandler
{
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "next is common name for middlewares")]
    Task<IResponseWrapper?> ProcessJsonRpcRequest(IRequestWrapper? requestWrapper, HttpContext httpContext, RequestDelegate next);
}
