using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.Services
{
    /// <summary>
    /// Creates Errors by specification rules, wraps exceptions depending on options
    /// </summary>
    public interface IJsonRpcErrorFactory
    {
        /// <summary>
        /// -32004 Similar to 404
        /// </summary>
        /// <param name="errorData"></param>
        /// <returns></returns>
        IError NotFound(object? errorData);

        /// <summary>
        /// -32600 The JSON sent is not a valid Request object. From specification.
        /// </summary>
        /// <param name="errorData"></param>
        /// <returns></returns>
        IError InvalidRequest(object? errorData);

        /// <summary>
        /// -32601 The method does not exist. From specification.
        /// </summary>
        /// <param name="errorData"></param>
        /// <returns></returns>
        IError MethodNotFound(object? errorData);

        /// <summary>
        /// -32602 Invalid method parameter(s). From specification.
        /// </summary>
        /// <param name="errorData"></param>
        /// <returns></returns>
        IError InvalidParams(object? errorData);

        /// <summary>
        /// -32603 Internal JSON-RPC Error. From specification.
        /// </summary>
        /// <param name="errorData"></param>
        /// <returns></returns>
        IError InternalError(object? errorData);

        /// <summary>
        /// -32700 Invalid JSON was received by the server. From specification.
        /// </summary>
        /// <param name="errorData"></param>
        /// <returns></returns>
        IError ParseError(object? errorData);

        /// <summary>
        /// JSON Rpc Error response
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="errorData"></param>
        /// <returns></returns>
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
        IError Error(int code, string message, object? errorData);

        /// <summary>
        /// -32000 Exception as Server Error
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        IError Exception(Exception e);

        /// <summary>
        /// [-32099, -32000] JsonRpc server Errors in allowed range
        /// </summary>
        /// <param name="code"></param>
        /// <param name="errorData"></param>
        /// <returns></returns>
        IError ServerError(int code, object? errorData);

        /// <summary>
        /// Map HTTP response codes to pre-defined Errors from JsonRpc specification
        /// </summary>
        /// <returns></returns>
        IError HttpError(int httpCode, object? errorData);
    }
}
