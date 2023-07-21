using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server.Settings;

/// <summary>
/// Options to configure JSON-RPC server
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed class JsonRpcServerOptions
{
    /// <summary>
    /// Route prefix for all JSON-RPC requests. Required for correct routing.
    /// </summary>
    /// <remarks>
    /// <see cref="JsonRpcConstants.DefaultRoutePrefix" /> by default.<br />
    /// Can be set to `"/"` to get rid of it.
    /// </remarks>
    public PathString RoutePrefix { get; set; } = JsonRpcConstants.DefaultRoutePrefix;

    /// <summary>
    /// <see cref="JsonSerializerOptions" /> for serialization/deserialization of JSON Rpc "headers": `id`, `jsonrpc`, etc.
    /// </summary>
    /// <remarks>
    /// Changing this not recommended, because request/response "header" object format is fixed and does not imply any changes
    /// </remarks>
    public JsonSerializerOptions HeadersJsonSerializerOptions { get; set; } = JsonRpcSerializerOptions.Headers;

    /// <summary>
    /// <see cref="JsonSerializerOptions" /> for serialization of `params` and `method`
    /// and deserialization of `result` or `error.data`
    /// </summary>
    /// <remarks>
    /// <see cref="JsonRpcSerializerOptions.SnakeCase" /> by default
    /// </remarks>
    public JsonSerializerOptions DefaultDataJsonSerializerOptions { get; set; } = JsonRpcSerializerOptions.SnakeCase;

    /// <summary>
    /// Default <see cref="JsonRpcMethodStyle" /> for all actions
    /// </summary>
    /// <remarks>
    /// <see cref="JsonRpcMethodStyle.ControllerAndAction" /> by default
    /// </remarks>
    public JsonRpcMethodStyle DefaultMethodStyle { get; set; } = JsonRpcMethodStyle.ControllerAndAction;

    /// <summary>
    /// If `true`, exceptions are serialized with their `.ToString()` which includes stack trace
    /// </summary>
    /// <remarks>
    /// false by default
    /// </remarks>
    public bool DetailedResponseExceptions { get; set; }

    /// <summary>
    /// If `true`, server is allowed to return non JSON Rpc responses, like HTTP redirects, binary content, etc
    /// </summary>
    /// <remarks>
    /// false by default<br />
    /// Batches will break if this option is enabled and one of requests returns non-json data!
    /// </remarks>
    public bool AllowRawResponses { get; set; }
}
