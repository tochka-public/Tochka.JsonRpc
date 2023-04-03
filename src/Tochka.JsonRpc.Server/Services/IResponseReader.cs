using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Server.Models.Response;

namespace Tochka.JsonRpc.Server.Services;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Call is official name")]
public interface IResponseReader
{
    Task<IServerResponseWrapper> GetResponse(HttpContext nestedHttpContext, IUntypedCall call, bool allowRawResponses, CancellationToken token);
}