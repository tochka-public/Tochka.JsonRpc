using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Server.Services;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Next is conventional name for middleware delegates")]
public interface IRequestHandler
{
    Task HandleRequest(HttpContext httpContext, IRequestWrapper requestWrapper, Encoding requestEncoding, RequestDelegate next);
}