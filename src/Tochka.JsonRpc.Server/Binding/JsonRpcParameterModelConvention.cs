using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Binding;

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
        var position = parameter.ParameterInfo.Position;
        var fromParamsAttribute = parameter.Attributes.FirstOrDefault(static a => a is FromParamsAttribute) as FromParamsAttribute;
        var bindingStyle = fromParamsAttribute?.BindingStyle ?? BindingStyle.Default;
        var isOptional = parameter.ParameterInfo.IsOptional;
        foreach (var actionSelector in parameter.Action.Selectors)
        {
            var jsonSerializerOptions = Utils.GetDataJsonSerializerOptions(actionSelector.EndpointMetadata, options, serializerOptionsProviders);
            var propertyName = jsonSerializerOptions.PropertyNamingPolicy!.ConvertName(parameter.ParameterName);
            var metadata = actionSelector.EndpointMetadata.FirstOrDefault(static m => m is JsonRpcActionParametersMetadata);
            if (metadata is not JsonRpcActionParametersMetadata parametersMetadata) // == null
            {
                parametersMetadata = new JsonRpcActionParametersMetadata();
                actionSelector.EndpointMetadata.Add(parametersMetadata);
            }

            parametersMetadata.Parameters[parameter.ParameterName] = new JsonRpcParameterMetadata(propertyName, position, bindingStyle, isOptional);
        }
    }
}
