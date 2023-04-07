using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common;

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
    /// "result" JSON property to look for successful response data in single response
    /// </summary>
    public const string ResultProperty = "result";

    /// <summary>
    /// "error" JSON property to look for error in single response
    /// </summary>
    public const string ErrorProperty = "error";

    /// <summary>
    /// "code" JSON property inside "error"
    /// </summary>
    public const string ErrorCodeProperty = "code";

    /// <summary>
    /// "2.0" JSON Rpc version
    /// </summary>
    public const string Version = "2.0";

    /// <summary>
    /// "rpc." Specification restricts use of methods with this prefix
    /// </summary>
    public const string ReservedMethodPrefix = "rpc.";

    /// <summary>
    /// "rpc.discover" for OpenRPC Service Discovery
    /// </summary>
    public const string ServiceDiscoveryMethod = "rpc.discover";

    /// <summary>
    /// -32000 Server Error. Usually happens on unhandled Exception
    /// </summary>
    public const int ExceptionCode = -32000;

    /// <summary>
    /// -32001 Server Error. Usually caused by JsonRpcInternalException.
    /// May indicate that you are trying to return HTTP code or ActionResult not supported or disabled by your configuration, or an internal bug.
    /// </summary>
    public const int InternalExceptionCode = -32001;

    /// <summary>
    /// "api/jsonrpc" Default route for JSON Rpc controllers
    /// </summary>
    public const string DefaultRoute = "/api/jsonrpc";

    /// <summary>
    /// "api/jsonrpc" Default route prefix for JSON Rpc actions
    /// </summary>
    public const string DefaultRoutePrefix = "/api/jsonrpc";

    /// <summary>
    /// "application/json" Default content-type expected in JSON Rpc HTTP requests
    /// </summary>
    public const string ContentType = "application/json";

    /// <summary>
    /// "JsonRpc" Prefix for documents like Swagger and OpenRpc
    /// </summary>
    public const string ApiDocumentName = "jsonrpc";

    /// <summary>
    /// "REST" Swagger document name for non-JsonRpc actions
    /// </summary>
    public const string DefaultSwaggerDoc = "rest";

    /// <summary>
    /// Restriction for logging long texts
    /// </summary>
    public const int LogStringLimit = 5000;

    /// <summary>
    /// Parsed IUntypedCall, stored in HttpContext.Items after successful reading of JSON Rpc request
    /// </summary>
    public static readonly object RequestItemKey = new();

    /// <summary>
    /// When set in HttpContext.Items, denotes that this is nested pipeline with copied HttpContext, started from JsonRpcMiddleware
    /// </summary>
    public static readonly object NestedPipelineItemKey = new();

    /// <summary>
    /// Type of ActionResult returned from MVC action or filter. Stored in HttpContext.Items
    /// </summary>
    public static readonly object ActionResultTypeItemKey = new();

    /// <summary>
    /// ControllerActionDescriptor calculated by routing. Stored in HttpContext.Items
    /// </summary>
    public static readonly object ActionDescriptorItemKey = new();

    /// <summary>
    /// Response error code. Stored in HttpContext.Items after response is written
    /// </summary>
    public static readonly object ResponseErrorCodeItemKey = new();
}
