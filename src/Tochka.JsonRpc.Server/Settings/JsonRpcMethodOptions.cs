using System;
using Microsoft.AspNetCore.Http;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Server.Settings
{
    public class JsonRpcMethodOptions
    {
        public JsonRpcMethodOptions()
        {
            RequestSerializer = typeof(SnakeCaseRpcSerializer);
            Route = JsonRpcConstants.DefaultRoute;
            MethodStyle = MethodStyle.ControllerAndAction;
        }

        public Type RequestSerializer { get; set; }

        public PathString Route { get; set; }
        
        public MethodStyle MethodStyle { get; set; }
    }
}