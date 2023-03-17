using System.Text;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.V1.Server.Services
{
    public interface INestedContextFactory
    {
        HttpContext Create(HttpContext context, IUntypedCall call, Encoding requestEncoding);
    }
}
