using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Client.Models;

public interface ISingleJsonRpcResult
{
    T? GetResponseOrThrow<T>();
    T? AsResponse<T>();
    bool HasError();
    Error<JsonDocument>? AsAnyError();
    Error<T>? AsTypedError<T>();
    Error<ExceptionInfo>? AsErrorWithExceptionInfo();
}
