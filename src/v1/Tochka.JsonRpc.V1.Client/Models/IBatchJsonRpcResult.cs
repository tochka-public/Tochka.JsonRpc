using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common.Models.Id;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.V1.Client.Models
{
    public interface IBatchJsonRpcResult
    {
        T GetResponseOrThrow<T>(IRpcId id);
        T AsResponse<T>(IRpcId id);
        bool HasError(IRpcId id);
        Error<JToken> AsAnyError(IRpcId id);
        Error<T> AsTypedError<T>(IRpcId id);
        Error<ExceptionInfo> AsErrorWithExceptionInfo(IRpcId id);
    }
}
