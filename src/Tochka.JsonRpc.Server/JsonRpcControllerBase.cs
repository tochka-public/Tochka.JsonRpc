using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.Server;

/// <inheritdoc />
/// <summary>
/// Base class for JSON-RPC controller
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[JsonRpcController]
public abstract class JsonRpcControllerBase : ControllerBase
{
}
