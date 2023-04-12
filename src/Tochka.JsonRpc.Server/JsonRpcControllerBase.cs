using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.Server;

[JsonRpcController]
public abstract class JsonRpcControllerBase : ControllerBase
{
}
