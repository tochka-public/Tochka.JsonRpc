using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.Server;

[ExcludeFromCodeCoverage]
[JsonRpcController]
public abstract class JsonRpcControllerBase : ControllerBase
{
}
