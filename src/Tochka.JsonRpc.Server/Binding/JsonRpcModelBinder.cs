using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

/// <inheritdoc />
/// <summary>
/// <see cref="IModelBinder" /> for JSON-RPC parameters
/// </summary>
[PublicAPI]
public class JsonRpcModelBinder : IModelBinder
{
    private readonly IJsonRpcParamsParser paramsParser;
    private readonly IJsonRpcParameterBinder parameterBinder;
    private readonly IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private readonly JsonRpcServerOptions options;

    public JsonRpcModelBinder(IJsonRpcParamsParser paramsParser, IJsonRpcParameterBinder parameterBinder, IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders, IOptions<JsonRpcServerOptions> options)
    {
        this.paramsParser = paramsParser;
        this.parameterBinder = parameterBinder;
        this.serializerOptionsProviders = serializerOptionsProviders;
        this.options = options.Value;
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var actionParametersMetadata = bindingContext.ActionContext.ActionDescriptor.EndpointMetadata.Get<JsonRpcActionParametersMetadata>();
        if (actionParametersMetadata == null)
        {
            throw new JsonRpcServerException($"{nameof(JsonRpcActionParametersMetadata)} not found in endpoint metadata, it should've been populated in {nameof(JsonRpcParameterModelConvention)} on application start");
        }

        if (!actionParametersMetadata.Parameters.TryGetValue(bindingContext.FieldName, out var parameterMetadata))
        {
            throw new JsonRpcServerException($"Not found metadata for parameter [{bindingContext.FieldName}], it should've been populated in {nameof(JsonRpcParameterModelConvention)} on application start");
        }

        var parseResult = await Parse(bindingContext, parameterMetadata);
        await SetResult(parseResult, bindingContext, parameterMetadata);
    }

    /// <summary>
    /// Parse JSON-RPC parameters using <see cref="IJsonRpcParamsParser" />
    /// </summary>
    /// <param name="bindingContext">Context with information for model binding and validation</param>
    /// <param name="parameterMetadata">Metadata for parameter to parse</param>
    /// <exception cref="JsonRpcServerException">If raw or parsed JSON-RPC call is missing from <see cref="HttpContext" /></exception>
    // internal for tests, protected for customization
    protected internal virtual Task<IParseResult> Parse(ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata)
    {
        var rawCall = bindingContext.HttpContext.GetRawJsonRpcCall();
        var call = bindingContext.HttpContext.GetJsonRpcCall();
        if (rawCall == null || call is not IUntypedCall untypedCall) // == null
        {
            throw new JsonRpcServerException("Not found json rpc call in HttpContext, maybe middleware is missing");
        }

        var parseResult = paramsParser.Parse(rawCall, untypedCall.Params, parameterMetadata);
        return Task.FromResult(parseResult);
    }

    /// <summary>
    /// Bind parameter parse result to action arguments using <see cref="IJsonRpcParameterBinder" />
    /// </summary>
    /// <param name="parseResult">JSON-RPC parameter parse result</param>
    /// <param name="bindingContext">Context with information for model binding and validation</param>
    /// <param name="parameterMetadata">Metadata for parameter to bind</param>
    // internal for tests, protected for customization
    protected internal virtual Task SetResult(IParseResult parseResult, ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata)
    {
        var endpointMetadata = bindingContext.ActionContext.ActionDescriptor.EndpointMetadata;
        var jsonSerializerOptions = ServerUtils.GetDataJsonSerializerOptions(endpointMetadata, options, serializerOptionsProviders);
        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);
        return Task.CompletedTask;
    }
}
