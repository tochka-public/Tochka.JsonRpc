using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common
{
    [ExcludeFromCodeCoverage]
    public static class JsonRpcConstants
    {
        /// <summary>
        /// Parsed IUntypedCall, stored in HttpContext.Items after successful reading of JSON Rpc request
        /// </summary>
        public static readonly object RequestItemKey = new object();

        /// <summary>
        /// When set in HttpContext.Items, denotes that this is nested pipeline with copied HttpContext, started from JsonRpcMiddleware
        /// </summary>
        public static readonly object NestedPipelineItemKey = new object();

        /// <summary>
        /// Type of ActionResult returned from MVC action or filter. Stored in HttpContext.Items
        /// </summary>
        public static readonly object ActionResultTypeItemKey = new object();

        /// <summary>
        /// "." Delimiter in "controller_name.action_name" to mimic "Class.Method" notation
        /// </summary>
        public static readonly string ControllerMethodSeparator = ".";

        /// <summary>
        /// "id" JSON property to look for Id in single request
        /// </summary>
        public static readonly string IdProperty = "id";

        /// <summary>
        /// "result" JSON property to look for successful response data in single response
        /// </summary>
        public static readonly string ResultProperty = "result";

        /// <summary>
        /// "error" JSON property to look for error in single response
        /// </summary>
        public static readonly string ErrorProperty = "error";

        /// <summary>
        /// "2.0" JSON Rpc version
        /// </summary>
        public static readonly string Version = "2.0";

        /// <summary>
        /// "rpc." Specification restricts use of methods with this prefix
        /// </summary>
        public static readonly string ReservedMethodPrefix = "rpc.";

        /// <summary>
        /// "rpc.discover" for OpenRPC Service Discovery
        /// </summary>
        public static readonly string ServiceDiscoveryMethod = "rpc.discover";

        /// <summary>
        /// -32000 Server Error. Usually happens on unhandled Exception
        /// </summary>
        public static readonly int ExceptionCode = -32000;

        /// <summary>
        /// -32001 Server Error. Usually caused by JsonRpcInternalException.
        /// May indicate that you are trying to return HTTP code or ActionResult not supported or disabled by your configuration, or an internal bug.
        /// </summary>
        public static readonly int InternalExceptionCode = -32001;

        /// <summary>
        /// "api/jsonrpc" Default route for JSON Rpc controllers
        /// </summary>
        public static readonly string DefaultRoute = "/api/jsonrpc";

        /// <summary>
        /// "application/json" Default content-type expected in JSON Rpc HTTP requests
        /// </summary>
        public static readonly string ContentType = "application/json";

        /// <summary>
        /// "JsonRpc" Prefix for swagger documents
        /// </summary>
        public static readonly string JsonRpcSwaggerPrefix = "JsonRpc";

        /// <summary>
        /// "REST" Swagger document name for non-JsonRpc actions
        /// </summary>
        public const string DefaultSwaggerDoc = "rest";

    }
}