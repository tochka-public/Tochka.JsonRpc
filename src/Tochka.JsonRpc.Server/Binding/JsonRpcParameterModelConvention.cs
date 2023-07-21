using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

/// <inheritdoc />
/// <summary>
/// <see cref="IParameterModelConvention" /> to add binding info and metadata to parameters of JSON-RPC endpoints
/// </summary>
internal class JsonRpcParameterModelConvention : IParameterModelConvention
{
    private readonly IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private readonly JsonRpcServerOptions options;

    public JsonRpcParameterModelConvention(IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders, IOptions<JsonRpcServerOptions> options)
    {
        this.serializerOptionsProviders = serializerOptionsProviders;
        this.options = options.Value;
    }

    public void Apply(ParameterModel parameter)
    {
        if (!parameter.Action.Controller.Attributes.Any(static a => a is JsonRpcControllerAttribute))
        {
            return;
        }

        parameter.BindingInfo ??= BindingInfo.GetBindingInfo(new[] { new FromParamsAttribute(BindingStyle.Default) });
        if (parameter.BindingInfo?.BinderType != typeof(JsonRpcModelBinder))
        {
            return;
        }

        var position = parameter.ParameterInfo.Position;
        var fromParamsAttribute = parameter.Attributes.Get<FromParamsAttribute>();
        var bindingStyle = fromParamsAttribute?.BindingStyle ?? BindingStyle.Default;
        var isOptional = parameter.ParameterInfo.IsOptional;
        foreach (var actionSelector in parameter.Action.Selectors)
        {
            var jsonSerializerOptions = ServerUtils.GetDataJsonSerializerOptions(actionSelector.EndpointMetadata, options, serializerOptionsProviders);
            var propertyName = jsonSerializerOptions.PropertyNamingPolicy!.ConvertName(parameter.ParameterName);
            var parametersMetadata = actionSelector.EndpointMetadata.Get<JsonRpcActionParametersMetadata>();
            if (parametersMetadata == null)
            {
                parametersMetadata = new JsonRpcActionParametersMetadata();
                actionSelector.EndpointMetadata.Add(parametersMetadata);
            }

            parametersMetadata.Parameters[parameter.ParameterName] = new JsonRpcParameterMetadata(propertyName, position, bindingStyle, isOptional, parameter.ParameterName, parameter.ParameterType);
        }
    }
}
