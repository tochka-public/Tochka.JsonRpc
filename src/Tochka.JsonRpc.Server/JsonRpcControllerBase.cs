using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.Server;

/// <inheritdoc />
/// <summary>
/// Base class for JSON-RPC controller
/// </summary>
[ExcludeFromCodeCoverage]
[JsonRpcController]
public abstract class JsonRpcControllerBase : ControllerBase
{
}
