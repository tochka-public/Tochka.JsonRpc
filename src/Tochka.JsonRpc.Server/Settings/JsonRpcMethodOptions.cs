using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server.Settings;

public class JsonRpcMethodOptions
{
    public JsonRpcMethodOptions()
    {
        RequestSerializer = typeof(SnakeCaseJsonRpcSerializer);
        Route = JsonRpcConstants.DefaultRoute;
        MethodStyle = MethodStyle.ControllerAndAction;
    }

    public Type RequestSerializer { get; set; }

    public PathString Route { get; set; }

    public MethodStyle MethodStyle { get; set; }
}