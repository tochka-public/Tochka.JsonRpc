using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

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
        var endpointMetadata = bindingContext.ActionContext.ActionDescriptor.EndpointMetadata;
        var metadata = endpointMetadata.FirstOrDefault(static m => m is JsonRpcActionParametersMetadata);
        if (metadata is not JsonRpcActionParametersMetadata actionParametersMetadata) // == null
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

    protected virtual Task<IParseResult> Parse(ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata)
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

    protected virtual Task SetResult(IParseResult parseResult, ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata)
    {
        var endpointMetadata = bindingContext.ActionContext.ActionDescriptor.EndpointMetadata;
        var jsonSerializerOptions = Utils.GetDataJsonSerializerOptions(endpointMetadata, options, serializerOptionsProviders);
        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);
        return Task.CompletedTask;
    }
}
