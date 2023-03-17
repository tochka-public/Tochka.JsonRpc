using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.V1.Common.Models.Request.Wrappers;

namespace Tochka.JsonRpc.V1.Server.Services
{
    public interface IRequestHandler
    {
        Task HandleRequest(HttpContext httpContext, IRequestWrapper requestWrapper, Encoding requestEncoding, RequestDelegate next);
    }
}
