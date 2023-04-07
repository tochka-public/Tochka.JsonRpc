using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server;

public class JsonRpcServerOptions
{
    public PathString RoutePrefix { get; set; } = JsonRpcConstants.DefaultRoutePrefix;
}
