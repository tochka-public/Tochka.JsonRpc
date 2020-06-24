using System.Text;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Server.Services
{
    public interface INestedContextFactory
    {
        HttpContext Create(HttpContext context, IUntypedCall call, Encoding requestEncoding);
    }
}