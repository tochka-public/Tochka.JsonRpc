using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common;

/// <summary>
/// All common JSON-RPC constants
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static class JsonRpcConstants
{
    /// <summary>
    /// "." Delimiter in "controller_name.action_name" to mimic "Class.Method" notation
    /// </summary>
    public const string ControllerMethodSeparator = ".";

    /// <summary>
    /// "id" JSON property to look for Id in single request/response
    /// </summary>
    public const string IdProperty = "id";

    /// <summary>
    /// "jsonrpc" JSON property to look for version in single request/response
    /// </summary>
    public const string JsonrpcVersionProperty = "jsonrpc";

    /// <summary>
    /// "method" JSON property to look for method in single request
    /// </summary>
    public const string MethodProperty = "method";

    /// <summary>
    /// "params" JSON property to look for parameters in single request
    /// </summary>
    public const string ParamsProperty = "params";

    /// <summary>
    /// "result" JSON property to look for successful response data in single response
    /// </summary>
    public const string ResultProperty = "result";

    /// <summary>
    /// "error" JSON property to look for error in single response
    /// </summary>
    public const string ErrorProperty = "error";

    /// <summary>
    /// "2.0" JSON Rpc version
    /// </summary>
    public const string Version = "2.0";

    /// <summary>
    /// -32000 Server Error. Usually happens on unhandled Exception
    /// </summary>
    public const int ExceptionCode = -32000;

    /// <summary>
    /// -32001 Server Error. Usually caused by JsonRpcInternalException.<br />
    /// May indicate that you are trying to return HTTP code or ActionResult not supported or disabled by your configuration, or an internal bug.
    /// </summary>
    public const int InternalExceptionCode = -32001;

    /// <summary>
    /// "api/jsonrpc" Default route prefix for JSON Rpc actions
    /// </summary>
    public const string DefaultRoutePrefix = "/api/jsonrpc";

    /// <summary>
    /// "application/json" Default content-type expected in JSON Rpc HTTP requests
    /// </summary>
    public const string ContentType = "application/json";

    /// <summary>
    /// Restriction for logging long texts
    /// </summary>
    public const int LogStringLimit = 5000;
}
