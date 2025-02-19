using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Services;

/// <summary>
/// Creates <see cref="IError" /> by specification rules, wraps exceptions depending on options
/// </summary>
public interface IJsonRpcErrorFactory
{
    /// <summary>
    /// -32700 Invalid JSON was received by the server. From specification.
    /// </summary>
    /// <param name="errorData">error.data</param>
    IError ParseError(object? errorData);

    /// <summary>
    /// -32600 The JSON sent is not a valid Request object. From specification.
    /// </summary>
    /// <param name="errorData">error.data</param>
    IError InvalidRequest(object? errorData);

    /// <summary>
    /// -32601 The method does not exist. From specification.
    /// </summary>
    /// <param name="errorData">error.data</param>
    IError MethodNotFound(object? errorData);

    /// <summary>
    /// -32602 Invalid method parameter(s). From specification.
    /// </summary>
    /// <param name="errorData">error.data</param>
    IError InvalidParams(object? errorData);

    /// <summary>
    /// -32603 Internal JSON-RPC Error. From specification.
    /// </summary>
    /// <param name="errorData">error.data</param>
    IError InternalError(object? errorData);

    /// <summary>
    /// [-32099, -32000] JsonRpc server Errors in allowed range
    /// </summary>
    /// <param name="code">error.code</param>
    /// <param name="errorData">error.data</param>
    /// <remarks>
    /// Value of <paramref name="code" /> must be in [-32099, -32000] interval
    /// </remarks>
    IError ServerError(int code, object? errorData);

    /// <summary>
    /// -32004 Similar to 404
    /// </summary>
    /// <param name="errorData">error.data</param>
    IError NotFound(object? errorData);

    /// <summary>
    /// JSON Rpc Error response
    /// </summary>
    /// <param name="code">error.code</param>
    /// <param name="message">error.message</param>
    /// <param name="errorData">error.data</param>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
    IError Error(int code, string message, object? errorData);

    /// <summary>
    /// -32000 Exception as Server Error
    /// </summary>
    /// <param name="e">Exception to get info from for error.data</param>
    IError Exception(Exception e);

    /// <summary>
    /// Map HTTP response codes to pre-defined Errors from JsonRpc specification
    /// </summary>
    /// <param name="httpCode">HTTP response status code</param>
    /// <param name="errorData">error.data</param>
    IError HttpError(int httpCode, object? errorData);
}
