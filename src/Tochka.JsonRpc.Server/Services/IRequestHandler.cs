using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.Server.Services
{
    public interface IRequestHandler
    {
        Task HandleRequest(HttpContext httpContext, IRequestWrapper requestWrapper, Encoding requestEncoding, RequestDelegate next);
    }
}