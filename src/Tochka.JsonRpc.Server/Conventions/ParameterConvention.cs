using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Conventions
{
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

        public void Apply(ParameterModel parameterModel)
        {
            // only mess with parameters of actions of JSON Rpc controllers
            if (!Utils.IsJsonRpcController(parameterModel.Action.Controller.ControllerType))
            {
                return;
            }

            if (!(parameterModel.Action.Properties[typeof(MethodMetadata)] is MethodMetadata methodMetadata))
            {
                throw new ArgumentNullException(nameof(methodMetadata));
            }
            
            var parameterMetadata = GetParameterMetadata(parameterModel, methodMetadata.MethodOptions.RequestSerializer);
            ValidateParameter(parameterModel, parameterMetadata.BindingStyle);
            // store metadata to help bind JSON Rpc params later
            methodMetadata.Add(parameterMetadata);
            SetBinding(parameterModel);
        }

        internal ParameterMetadata GetParameterMetadata(ParameterModel parameterModel, Type serializerType)
        {
            var rpcParams = Utils.GetAttribute<FromParamsAttribute>(parameterModel)?.BindingStyle ?? BindingStyle.Default;
            var serializer = Utils.GetSerializer(serializers, serializerType);
            var parameterName = serializer.GetJsonName(parameterModel.ParameterName);
            var isOptional = parameterModel.ParameterInfo.IsOptional; // see https://stackoverflow.com/q/9977530/
            var result = new ParameterMetadata(parameterName, parameterModel.ParameterType, parameterModel.ParameterInfo.Position, rpcParams, isOptional);
            log.LogTrace($"{parameterModel.DisplayName}: metadata [{result}]");
            return result;
        }

        internal void SetBinding(ParameterModel parameterModel)
        {
            if (parameterModel.BindingInfo == null)
            {
                parameterModel.BindingInfo = new BindingInfo()
                {
                    BinderType = typeof(JsonRpcModelBinder),
                    BindingSource = BindingSource.Custom
                };
            }
            log.LogTrace($"{parameterModel.DisplayName}: applied {nameof(JsonRpcModelBinder)}");
        }

        internal void ValidateParameter(ParameterModel parameterModel, BindingStyle bindingStyle)
        {
            var isCollection = parameterModel.ParameterType.GetInterface(nameof(ICollection)) != null;
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
}