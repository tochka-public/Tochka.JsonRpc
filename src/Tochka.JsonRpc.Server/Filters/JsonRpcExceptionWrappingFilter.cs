﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Server.Filters;

/// <inheritdoc />
/// <summary>
/// Filter for JSON-RPC actions to convert exceptions to JSON-RPC error
/// </summary>
internal class JsonRpcExceptionWrappingFilter : IExceptionFilter
{
    private readonly IJsonRpcErrorFactory errorFactory;

    public JsonRpcExceptionWrappingFilter(IJsonRpcErrorFactory errorFactory) => this.errorFactory = errorFactory;

    // wrap exception in json rpc error
    public void OnException(ExceptionContext context)
    {
        if (context.HttpContext.GetJsonRpcCall() == null)
        {
            return;
        }

        var error = errorFactory.Exception(context.Exception);
        context.Result = new ObjectResult(error);
    }
}
