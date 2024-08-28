using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services;

/// <inheritdoc />
[PublicAPI]
public class JsonRpcErrorFactory : IJsonRpcErrorFactory
{
    private readonly ILogger<JsonRpcErrorFactory> log;
    private readonly JsonRpcServerOptions options;

    /// <summary></summary>
    public JsonRpcErrorFactory(IOptions<JsonRpcServerOptions> options, ILogger<JsonRpcErrorFactory> log)
    {
        this.log = log;
        this.options = options.Value;
    }

    /// <inheritdoc />
    public IError ParseError(object? errorData) =>
        new Error<object>(JsonRpcErrorCodes.ParseError, "Parse error", WrapExceptions(errorData));

    /// <inheritdoc />
    public IError InvalidRequest(object? errorData) =>
        new Error<object>(JsonRpcErrorCodes.InvalidRequest, "Invalid Request", WrapExceptions(errorData));

    /// <inheritdoc />
    public IError MethodNotFound(object? errorData) =>
        new Error<object>(JsonRpcErrorCodes.MethodNotFound, "Method not found", WrapExceptions(errorData));

    /// <inheritdoc />
    public IError InvalidParams(object? errorData) =>
        new Error<object>(JsonRpcErrorCodes.InvalidParams, "Invalid params", WrapExceptions(errorData));

    /// <inheritdoc />
    public IError InternalError(object? errorData) =>
        new Error<object>(JsonRpcErrorCodes.InternalError, "Internal error", WrapExceptions(errorData));

    /// <inheritdoc />
    public IError ServerError(int code, object? errorData) =>
        !IsServer(code)
            ? throw new ArgumentOutOfRangeException(nameof(code), code, $"Expected code in server range [{-32099}, {-32000}]")
            : new Error<object>(code, "Server error", WrapExceptions(errorData));

    /// <inheritdoc />
    public virtual IError NotFound(object? errorData) =>
        new Error<object>(-32004, "Not found", WrapExceptions(errorData));

    /// <inheritdoc />
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
    public virtual IError Error(int code, string message, object? errorData) =>
        IsReserved(code)
            ? throw new ArgumentOutOfRangeException(nameof(code), code, "This code is in reserved range [-32768, -32000], use another")
            : new Error<object>(code, message, WrapExceptions(errorData));

    /// <inheritdoc />
    public virtual IError Exception(Exception e) => e switch
    {
        JsonRpcServerException => ServerError(JsonRpcConstants.InternalExceptionCode, WrapExceptions(e)),
        JsonRpcMethodNotFoundException methodException => MethodNotFound(new { methodException.Method }),
        JsonRpcErrorException errorException => errorException.Error,
        JsonRpcFormatException => InvalidRequest(WrapExceptions(e)),
        _ => ServerError(JsonRpcConstants.ExceptionCode, WrapExceptions(e))
    };

    /// <inheritdoc />
    public virtual IError HttpError(int httpCode, object? errorData) => httpCode switch
    {
        400 or 422 => InvalidParams(errorData),
        401 or 403 => MethodNotFound(errorData),
        404 => NotFound(errorData),
        415 => ParseError(errorData),
        500 => InternalError(errorData),
        _ => ServerError(JsonRpcConstants.InternalExceptionCode, errorData)
    };

    /// <summary>
    /// Hide stack trace if detailed response disabled, avoid serializing exceptions directly
    /// </summary>
    /// <param name="errorData">error.data that could be exception</param>
    // internal for tests, protected for customization
    protected internal virtual object? WrapExceptions(object? errorData)
    {
        if (errorData is not Exception e)
        {
            return errorData;
        }

        object? details = null;
        if (options.DetailedResponseExceptions)
        {
            log.LogTrace("Wrap detailed exception [{exceptionTypeName}]", e.GetType().Name);
            details = e.ToString();
        }
        else
        {
            log.LogTrace("Wrap exception without details [{exceptionTypeName}]", e.GetType().Name);
        }

        return new ExceptionInfo(GetExceptionTypeName(e), e.Message, details);
    }

    /// <summary>
    /// Check if code is reserved in specification
    /// </summary>
    /// <param name="code">error.code</param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    protected static bool IsReserved(int code) => code is >= -32768 and <= -32000;

    /// <summary>
    /// Check if code is explicitly defined in specification
    /// </summary>
    /// <param name="code">error.code</param>
    [ExcludeFromCodeCoverage]
    protected static bool IsSpecial(int code) => code is JsonRpcErrorCodes.ParseError
        or JsonRpcErrorCodes.InvalidRequest
        or JsonRpcErrorCodes.MethodNotFound
        or JsonRpcErrorCodes.InvalidParams
        or JsonRpcErrorCodes.InternalError;

    /// <summary>
    /// Check if code is reserved for server implementation in specification
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    protected static bool IsServer(int code) => code is >= -32099 and <= -32000;

    [ExcludeFromCodeCoverage(Justification = "FullName == null is probably impossible case, left it for sanity")]
    private static string GetExceptionTypeName(Exception exception) => exception.GetType().FullName ?? exception.GetType().Name;
}
