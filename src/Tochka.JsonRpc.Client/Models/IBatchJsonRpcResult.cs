using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Client.Models;

public interface IBatchJsonRpcResult
{
    T? GetResponseOrThrow<T>(IRpcId? id);
    T? AsResponse<T>(IRpcId? id);
    bool HasResponse(IRpcId? id);
    bool HasError(IRpcId? id);
    Error<JsonDocument>? AsUntypedError(IRpcId? id);
    Error<T>? AsError<T>(IRpcId? id);
    Error<ExceptionInfo>? AsErrorWithExceptionInfo(IRpcId? id);
}
