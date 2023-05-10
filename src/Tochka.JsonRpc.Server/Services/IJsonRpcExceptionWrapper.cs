using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Server.Services;

public interface IJsonRpcExceptionWrapper
{
    UntypedErrorResponse WrapGeneralException(Exception exception, IRpcId? id = null);
    UntypedErrorResponse WrapParseException(Exception exception);
}
