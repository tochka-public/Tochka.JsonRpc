using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Server.Models.Response;

namespace Tochka.JsonRpc.V1.Server.Services
{
    public interface IResponseReader
    {
        Task<IServerResponseWrapper> GetResponse(HttpContext nestedHttpContext, IUntypedCall call, bool allowRawResponses, CancellationToken token);
    }
}
