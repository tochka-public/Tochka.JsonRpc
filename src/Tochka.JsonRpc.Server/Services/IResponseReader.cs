using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Server.Models.Response;

namespace Tochka.JsonRpc.Server.Services
{
    public interface IResponseReader
    {
        Task<IServerResponseWrapper> GetResponse(HttpContext nestedHttpContext, IUntypedCall call, bool allowRawResponses, CancellationToken token);
    }
}