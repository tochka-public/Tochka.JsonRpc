using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Filters;

/// <inheritdoc />
/// <summary>
/// Filter for JSON-RPC actions to to convert <see cref="IActionResult" /> to JSON-RPC responses
/// </summary>
internal class JsonRpcResultFilter : IAlwaysRunResultFilter
{
    private readonly IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private readonly JsonRpcServerOptions serverOptions;
    private readonly IJsonRpcErrorFactory errorFactory;

    public JsonRpcResultFilter(IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders, IOptions<JsonRpcServerOptions> options, IJsonRpcErrorFactory errorFactory)
    {
        this.serializerOptionsProviders = serializerOptionsProviders;
        serverOptions = options.Value;
        this.errorFactory = errorFactory;
    }

    // wrap action results in json rpc response
    public void OnResultExecuting(ResultExecutingContext context)
    {
        var call = context.HttpContext.GetJsonRpcCall();
        if (call is UntypedNotification)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status200OK);
            return;
        }

        if (call is not UntypedRequest request) // == null
        {
            return;
        }

        var jsonSerializerOptions = ServerUtils.GetDataJsonSerializerOptions(context.ActionDescriptor.EndpointMetadata, serverOptions, serializerOptionsProviders);
        var response = GetResult(context.Result);
        if (response is IActionResult)
        {
            if (!serverOptions.AllowRawResponses)
            {
                throw new JsonRpcServerException($"Raw responses are not allowed by default. If you want to use them, set {nameof(JsonRpcServerOptions.AllowRawResponses)} = true in configuration");
            }

            if (context.HttpContext.JsonRpcRequestIsBatch())
            {
                throw new JsonRpcServerException("Raw responses are not allowed in batch requests");
            }

            return;
        }

        context.HttpContext.SetJsonRpcResponse(response switch
        {
            IError error => new UntypedErrorResponse(request.Id, error.AsUntypedError(jsonSerializerOptions)),
            _ => new UntypedResponse(request.Id, SerializeData(response, jsonSerializerOptions))
        });
        context.Result = new StatusCodeResult(StatusCodes.Status200OK);
    }

    [ExcludeFromCodeCoverage]
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private object? GetResult(IActionResult actionResult) => actionResult switch
    {
        ObjectResult { Value: IError error } => error,
        ObjectResult { StatusCode: >= 400 } result => errorFactory.HttpError(result.StatusCode.Value, result.Value),
        ObjectResult result => result.Value,
        StatusCodeResult { StatusCode: >= 400 } result => errorFactory.HttpError(result.StatusCode, null),
        EmptyResult => null,
        _ => actionResult
    };

    private static JsonDocument? SerializeData(object? result, JsonSerializerOptions jsonSerializerOptions) =>
        result == null
            ? null
            : JsonSerializer.SerializeToDocument(result, jsonSerializerOptions);
}
