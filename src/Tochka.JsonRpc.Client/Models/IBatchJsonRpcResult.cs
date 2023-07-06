using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Client.Models;

[PublicAPI]
public interface IBatchJsonRpcResult
{
    TResponse? GetResponseOrThrow<TResponse>(IRpcId id);
    TResponse? AsResponse<TResponse>(IRpcId id);
    bool HasError(IRpcId id);
    Error<JsonDocument>? AsAnyError(IRpcId id);
    Error<TError>? AsTypedError<TError>(IRpcId id);
    Error<ExceptionInfo>? AsErrorWithExceptionInfo(IRpcId id);
}
