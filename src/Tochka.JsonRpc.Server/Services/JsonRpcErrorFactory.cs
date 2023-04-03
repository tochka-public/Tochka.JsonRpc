using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services;

/// <summary>
/// Creates errors by specification rules, wraps exceptions depending on options
/// </summary>
internal class JsonRpcErrorFactory : IJsonRpcErrorFactory
{
    private readonly ILogger log;
    private readonly JsonRpcOptions options;

    public JsonRpcErrorFactory(IOptions<JsonRpcOptions> options, ILogger<JsonRpcErrorFactory> log)
    {
        this.log = log;
        this.options = options.Value;
    }

    /// <inheritdoc />
    public virtual IError NotFound(object errorData)
    {
        return new Error<object>(-32004, "Not found", WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError InvalidRequest(object errorData)
    {
        return new Error<object>(-32600, "Invalid Request", WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError MethodNotFound(object errorData)
    {
        return new Error<object>(-32601, "Method not found", WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError InvalidParams(object errorData)
    {
        return new Error<object>(-32602, "Invalid params", WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError InternalError(object errorData)
    {
        return new Error<object>(-32603, "Internal error", WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError ParseError(object errorData)
    {
        return new Error<object>(-32700, "Parse error", WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError Error(int code, string message, object errorData)
    {
        if (IsReserved(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "This code is in reserved range [-32768, -32000], use another");
        }

        return new Error<object>(code, message, WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError Exception(Exception e)
    {
        if (e is JsonRpcInternalException x)
        {
            return InternalException(x);
        }
        return ServerError(JsonRpcConstants.ExceptionCode, WrapExceptions(e));
    }

    /// <inheritdoc />
    public virtual IError ServerError(int code, object errorData)
    {
        if (!IsServer(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, $"Expected code in server range [{-32099}, {-32000}]");
        }

        return new Error<object>(code, "Server error", WrapExceptions(errorData));
    }

    /// <inheritdoc />
    public virtual IError HttpError(int? httpCode, object errorData)
    {
        switch (httpCode)
        {
            case 400:
            case 422:
                return InvalidParams(errorData);
            case 401:
            case 403:  // no known IActionResult returns this?
                return MethodNotFound(errorData);
            case 404:
                // TODO distinguish between failed routing and returned NotFoundResult
                return NotFound(errorData);
            case 415:
                return ParseError(errorData);
            case 500:
                return InternalError(errorData);
            default:
                return ServerError(JsonRpcConstants.InternalExceptionCode, errorData);
        }
    }

    /// <summary>
    /// -32001 Internal Exception as Server Error
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    internal virtual IError InternalException(JsonRpcInternalException e)
    {
        return HttpError(e.HttpCode, WrapExceptions(e, e.HttpCode));
    }

    /// <summary>
    /// Hide stack trace if detailed response disabled, avoid serializing exceptions directly
    /// </summary>
    /// <param name="errorData"></param>
    /// <param name="httpCode"></param>
    /// <returns></returns>
    protected internal virtual object WrapExceptions(object errorData, int? httpCode=null)
    {
        switch (errorData)
        {
            case Exception e when options.DetailedResponseExceptions:
                log.LogTrace("Wrap detailed exception [{exceptionTypeName}]", e.GetType().Name);
                return new ExceptionInfo(httpCode, e.Message, e.GetType().FullName, e.ToString());
            case Exception e:
                log.LogTrace("Wrap exception without details [{exceptionTypeName}]", e.GetType().Name);
                return new ExceptionInfo(httpCode, e.Message, e.GetType().FullName, null);
            default:
                return errorData;
        }
    }

    /// <summary>
    /// Codes reserved in specification
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    protected internal static bool IsReserved(int code) => -32768 <= code && code <= -32000;

    /// <summary>
    /// Codes explicitly defined in specification
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    protected internal static bool IsSpecial(int code) => code == -32700 || code == -32600 || code == -32601 || code == -32602 || code == -32603;

    /// <summary>
    /// Codes reserved for server implementation in specification
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    protected internal static bool IsServer(int code) => -32099 <= code && code <= -32000;
}