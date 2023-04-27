using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

internal class JsonRpcModelBinder : IModelBinder
{
    private readonly IJsonRpcParamsParser parser;
    private readonly IParameterBinder parameterBinder;
    private readonly IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private readonly JsonRpcServerOptions options;

    public JsonRpcModelBinder(IJsonRpcParamsParser parser, IParameterBinder parameterBinder, IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders, IOptions<JsonRpcServerOptions> options)
    {
        this.parser = parser;
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

        var jsonSerializerOptions = Utils.GetDataJsonSerializerOptions(endpointMetadata, options, serializerOptionsProviders);
        var rawCall = bindingContext.HttpContext.GetRawJsonRpcCall();
        var call = bindingContext.HttpContext.GetJsonRpcCall();
        if (rawCall == null || call is not IUntypedCall untypedCall) // == null
        {
            throw new JsonRpcServerException($"Not found json rpc call in HttpContext, maybe middleware is missing");
        }

        var parseResult = parser.Parse(rawCall, untypedCall.Params, parameterMetadata);
        parameterBinder.SetResult(bindingContext, parameterMetadata, parseResult, jsonSerializerOptions);
    }
}
