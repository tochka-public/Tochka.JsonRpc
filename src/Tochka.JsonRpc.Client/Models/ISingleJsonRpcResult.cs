using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Client.Models;

[PublicAPI]
public interface ISingleJsonRpcResult
{
    TResponse? GetResponseOrThrow<TResponse>();
    TResponse? AsResponse<TResponse>();
    bool HasError();
    Error<JsonDocument>? AsAnyError();
    Error<TError>? AsTypedError<TError>();
    Error<ExceptionInfo>? AsErrorWithExceptionInfo();
}
