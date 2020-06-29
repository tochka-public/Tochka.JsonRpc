using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Conventions
{
    /// <summary>
    /// Match JSON Rpc requests to actions
    /// </summary>
    public class ActionConvention : IActionModelConvention
    {
        private readonly IEnumerable<IRpcSerializer> serializers;
        private readonly IMethodMatcher methodMatcher;
        private readonly ILogger log;
        private readonly JsonRpcMethodOptions defaultMethodOptions;

        public ActionConvention(IOptions<JsonRpcOptions> options, IEnumerable<IRpcSerializer> serializers, IMethodMatcher methodMatcher, ILogger<ActionConvention> log)
        {
            this.serializers = serializers;
            this.methodMatcher = methodMatcher;
            this.log = log;
            defaultMethodOptions = options.Value.DefaultMethodOptions;
        }

        public void Apply(ActionModel actionModel)
        {
            // only mess with actions of JSON Rpc controllers
            if (!Utils.IsJsonRpcController(actionModel.Controller.ControllerType))
            {
                return;
            }

            var methodOptions = MakeOptions(actionModel);
            var methodMetadata = GetMethodMetadata(actionModel, methodOptions);
            SetAttributes(actionModel, methodOptions);
            ValidateRouting(methodMetadata);

            // store metadata to help bind JSON Rpc params later
            actionModel.Properties[typeof(MethodMetadata)] = methodMetadata;
        }

        internal MethodMetadata GetMethodMetadata(ActionModel actionModel, JsonRpcMethodOptions methodOptions)
        {
            var serializer = Utils.GetSerializer(serializers, methodOptions.RequestSerializer);
            var controllerName = serializer.GetJsonName(actionModel.Controller.ControllerName);
            var actionName = serializer.GetJsonName(actionModel.ActionName);
            var result = new MethodMetadata(methodOptions, controllerName, actionName);
            log.LogTrace($"{actionModel.DisplayName}: metadata [{result}]");
            return result;
        }

        /// <summary>
        /// Replace framework conventions with explicit route and action selector
        /// </summary>
        /// <param name="actionModel"></param>
        /// <param name="methodOptions"></param>
        internal void SetAttributes(ActionModel actionModel, JsonRpcMethodOptions methodOptions)
        {
            actionModel.Selectors.Clear();
            actionModel.Selectors.Add(new SelectorModel()
            {
                AttributeRouteModel = new AttributeRouteModel() {Template = methodOptions.Route},
                ActionConstraints = {new JsonRpcAttribute()}
            });
            log.LogTrace($"{actionModel.DisplayName}: applied {nameof(JsonRpcAttribute)}, route [{methodOptions.Route}]");
        }

        /// <summary>
        /// Get method-specific options from attributes or defaults
        /// </summary>
        /// <param name="actionModel"></param>
        /// <returns></returns>
        internal JsonRpcMethodOptions MakeOptions(ActionModel actionModel)
        {
            var serializerType = Utils.GetAttributeTransitive<JsonRpcSerializerAttribute>(actionModel)?.SerializerType ?? defaultMethodOptions.RequestSerializer;
            var route = Utils.GetAttributeTransitive<RouteAttribute>(actionModel)?.Template ?? defaultMethodOptions.Route;
            var method = Utils.GetAttributeTransitive<RpcMethodStyleAttribute>(actionModel)?.MethodStyle ?? defaultMethodOptions.MethodStyle;
            log.LogTrace($"{actionModel.DisplayName}: options are [{serializerType.FullName}], [{route}], [{method}]");
            return new JsonRpcMethodOptions()
            {
                RequestSerializer = serializerType,
                Route = route,
                MethodStyle = method,
            };
        }

        /// <summary>
        /// Prevent ambiguous and restricted routes
        /// </summary>
        internal void ValidateRouting(MethodMetadata methodMetadata)
        {
            var actionName = methodMatcher.GetActionName(methodMetadata);
            var key = $"{methodMetadata.MethodOptions.Route}?{actionName}";
            if (RegisteredRoutes.TryGetValue(key, out var metadata))
            {
                throw new InvalidOperationException($"Route [{methodMetadata.MethodOptions.Route}], method [{methodMetadata}] conflicts with already mapped [{metadata}]");
            }

            if (actionName.StartsWith(JsonRpcConstants.ReservedMethodPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Route [{methodMetadata.MethodOptions.Route}], method [{methodMetadata}] starts with reserved prefix [{JsonRpcConstants.ReservedMethodPrefix}]");
            }

            RegisteredRoutes.Add(key, methodMetadata);
            log.LogTrace($"Registered route key [{key}]");
        }

        internal readonly Dictionary<string, MethodMetadata> RegisteredRoutes = new Dictionary<string, MethodMetadata>(StringComparer.InvariantCultureIgnoreCase);
    }
}