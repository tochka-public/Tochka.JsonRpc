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

        if (call is not UntypedRequest request)
        {
            return;
        }

        var jsonSerializerOptions = Utils.GetDataJsonSerializerOptions(context.ActionDescriptor.EndpointMetadata, serverOptions, serializerOptionsProviders);
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
            IError error => new UntypedErrorResponse(request.Id, SerializeError(error, jsonSerializerOptions)),
            _ => new UntypedResponse(request.Id, SerializeData(response, jsonSerializerOptions))
        });
        context.Result = new StatusCodeResult(StatusCodes.Status200OK);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static Error<JsonDocument> SerializeError(IError error, JsonSerializerOptions jsonSerializerOptions) =>
        new Error<JsonDocument>(error.Code, error.Message, SerializeData(error.Data, jsonSerializerOptions));

    private static JsonDocument? SerializeData(object? result, JsonSerializerOptions jsonSerializerOptions) =>
        result == null
            ? null
            : JsonSerializer.SerializeToDocument(result, jsonSerializerOptions);

    private object? GetResult(IActionResult actionResult) => actionResult switch
    {
        ObjectResult { Value: IError error } => error,
        ObjectResult { StatusCode: < 200 or > 299 } result => errorFactory.HttpError(result.StatusCode.Value, result.Value),
        ObjectResult result => result.Value,
        StatusCodeResult { StatusCode: < 200 or > 299 } result => errorFactory.HttpError(result.StatusCode, null),
        EmptyResult => null,
        _ => actionResult
    };
}
