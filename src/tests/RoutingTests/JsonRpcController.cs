using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace RoutingTests;

public class JsonRpcController : JsonRpcControllerBase
{
    public ActionResult<object> ProcessAnything() =>
        new { LolKek = "Hello World!" };
        // throw new ArgumentException("Hello World!");
        // new FileContentResult(new byte[] { 1, 2, 3 }, "application/octet-stream");

        [JsonRpcSerializerOptions(typeof(SnakeCaseJsonSerializerOptionsProvider))]
        public ActionResult<object> SnakeCase() =>
            new { LolKek = "Hello World!" };

        [JsonRpcMethodStyle(JsonRpcMethodStyle.ControllerAndAction)]
        public ActionResult<object> ControllerAndAction() =>
            new { LolKek = "Hello World!" };

        [JsonRpcMethod("lol")]
        public ActionResult<object> CustomMethod() =>
            new { LolKek = "Hello World!" };
}
