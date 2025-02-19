using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Server.Services;

/// <summary>
/// Service to wrap exceptions in JSON-RPC error response
/// </summary>
public interface IJsonRpcExceptionWrapper
{
    /// <summary>
    /// Wrap any exception using <see cref="IJsonRpcErrorFactory" />
    /// </summary>
    /// <param name="exception">Exception to wrap</param>
    /// <param name="id">JSON-RPC request id</param>
    UntypedErrorResponse WrapGeneralException(Exception exception, IRpcId? id = null);

    /// <summary>
    /// Wrap exception as Parse Error using <see cref="IJsonRpcErrorFactory" />
    /// </summary>
    /// <param name="exception">Exception to wrap</param>
    UntypedErrorResponse WrapParseException(Exception exception);
}
