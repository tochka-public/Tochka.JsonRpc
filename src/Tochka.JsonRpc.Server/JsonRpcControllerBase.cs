using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Server;

/// <inheritdoc />
/// <summary>
/// Base class for JSON-RPC controller
/// </summary>
[ExcludeFromCodeCoverage]
[JsonRpcController]
public abstract class JsonRpcControllerBase : ControllerBase
{
    public IJsonRpcErrorFactory JsonRpcErrors => jsonRpcErrors ??= HttpContext.RequestServices.GetRequiredService<IJsonRpcErrorFactory>();

    private IJsonRpcErrorFactory? jsonRpcErrors;
}
