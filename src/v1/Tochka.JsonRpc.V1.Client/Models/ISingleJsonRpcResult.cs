using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.V1.Client.Models
{
    public interface ISingleJsonRpcResult
    {
        T GetResponseOrThrow<T>();
        T AsResponse<T>();
        bool HasError();
        Error<JToken> AsAnyError();
        Error<T> AsTypedError<T>();
        Error<ExceptionInfo> AsErrorWithExceptionInfo();
    }
}
