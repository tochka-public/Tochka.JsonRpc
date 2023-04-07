using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server;

[JsonRpcController]
[Route(JsonRpcConstants.DefaultRoute)]
public abstract class JsonRpcControllerBase : ControllerBase
{
}
