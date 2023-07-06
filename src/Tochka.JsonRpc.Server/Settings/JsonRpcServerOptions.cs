using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server.Settings;

[ExcludeFromCodeCoverage]
public sealed class JsonRpcServerOptions
{
    public PathString RoutePrefix { get; set; } = JsonRpcConstants.DefaultRoutePrefix;
    public JsonSerializerOptions HeadersJsonSerializerOptions { get; set; } = JsonRpcSerializerOptions.Headers;
    public JsonSerializerOptions DefaultDataJsonSerializerOptions { get; set; } = JsonRpcSerializerOptions.SnakeCase;
    public JsonRpcMethodStyle DefaultMethodStyle { get; set; } = JsonRpcMethodStyle.ControllerAndAction;
    public bool DetailedResponseExceptions { get; set; }
    public bool AllowRawResponses { get; set; }
}
