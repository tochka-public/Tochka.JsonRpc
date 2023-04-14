using System.Text;
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

    [JsonRpcSerializerOptions(typeof(CamelCaseJsonSerializerOptionsProvider))]
    public ActionResult<object> CamelCase() =>
        new { LolKek = "Hello World!" };

    [JsonRpcMethodStyle(JsonRpcMethodStyle.ControllerAndAction)]
    public ActionResult<object> ControllerAndAction() =>
        new { LolKek = "Hello World!" };

    [JsonRpcMethod("lol")]
    public ActionResult<object> CustomMethod() =>
        new { LolKek = "Hello World!" };

    public ActionResult<object> Exception() =>
        throw new ArgumentException("Hello World!");

    public ActionResult<object> RawResult() =>
        new FileContentResult(Encoding.UTF8.GetBytes("bytes"), "application/octet-stream");

    public ActionResult<object> Redirect() =>
        Redirect("/");

    public void Void()
    {
    }

    public ActionResult<object>? Null() => null;
}
