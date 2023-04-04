using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Conventions;

/// <summary>
/// Collect parameter metadata for request matching
/// </summary>
public class ParameterConvention : IParameterModelConvention
{
    private readonly IEnumerable<IJsonRpcSerializer> serializers;
    private readonly ILogger log;

    public ParameterConvention(IEnumerable<IJsonRpcSerializer> serializers, ILogger<ParameterConvention> log)
    {
        this.serializers = serializers;
        this.log = log;
    }

    public void Apply(ParameterModel parameter)
    {
        // only mess with parameters of actions of JSON Rpc controllers
        if (!Utils.IsJsonRpcController(parameter.Action.Controller.ControllerType))
        {
            return;
        }

        if (!(parameter.Action.Properties[typeof(MethodMetadata)] is MethodMetadata methodMetadata))
        {
            throw new ArgumentNullException(nameof(methodMetadata));
        }

        var parameterMetadata = GetParameterMetadata(parameter, methodMetadata.MethodOptions.RequestSerializer);
        ValidateParameter(parameter, parameterMetadata.BindingStyle);
        // store metadata to help bind JSON Rpc params later
        methodMetadata.Add(parameterMetadata);
        SetBinding(parameter);
    }

    internal ParameterMetadata GetParameterMetadata(ParameterModel parameterModel, Type serializerType)
    {
        var rpcParams = Utils.GetAttribute<FromParamsAttribute>(parameterModel)?.BindingStyle ?? BindingStyle.Default;
        var serializer = Utils.GetSerializer(serializers, serializerType);
        var parameterName = serializer.GetJsonName(parameterModel.ParameterName);
        // TODO use RequiredAttribute?
        var isOptional = parameterModel.ParameterInfo.IsOptional; // see https://stackoverflow.com/q/9977530/
        var result = new ParameterMetadata(parameterName, parameterModel.ParameterType, parameterModel.ParameterInfo.Position, rpcParams, isOptional);
        log.LogTrace("{parameterName}: metadata [{metadata}]", parameterModel.DisplayName, result);
        return result;
    }

    internal void SetBinding(ParameterModel parameterModel)
    {
        if (parameterModel.BindingInfo == null)
        {
            parameterModel.BindingInfo = new BindingInfo
            {
                BinderType = typeof(JsonRpcModelBinder),
                BindingSource = BindingSource.Custom
            };
        }
        log.LogTrace("{parameterName}: applied {binder}", parameterModel.DisplayName, nameof(JsonRpcModelBinder));
    }

    internal void ValidateParameter(ParameterModel parameterModel, BindingStyle bindingStyle)
    {
        var isCollection = Common.Utils.IsCollection(parameterModel.ParameterType);
        switch (bindingStyle)
        {
            case BindingStyle.Default:
                return;
            case BindingStyle.Object:
                // meh, dont want to check if complex type
                return;
            case BindingStyle.Array:
                if (!isCollection)
                {
                    throw new ArgumentOutOfRangeException(parameterModel.Name, parameterModel.ParameterType.Name, $"[{nameof(BindingStyle)}.{nameof(BindingStyle.Array)}] only works with collections. Change signature of [{parameterModel.Action.Controller.ControllerName}.{parameterModel.Action.ActionName}]");
                }
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(bindingStyle), bindingStyle, null);
        }
    }
}
