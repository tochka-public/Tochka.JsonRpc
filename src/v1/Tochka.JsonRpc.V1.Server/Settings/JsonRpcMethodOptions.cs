using System;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Server.Settings
{
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
}
