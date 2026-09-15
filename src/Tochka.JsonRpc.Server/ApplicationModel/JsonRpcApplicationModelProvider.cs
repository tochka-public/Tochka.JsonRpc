using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.ApplicationModel;

internal class JsonRpcApplicationModelProvider
(
    IOptions<JsonOptions> jsonOptions,
    IOptions<JsonRpcServerOptions> serverOptions
) : IApplicationModelProvider
{
    /// <remarks>
    /// Should run before ApiBehaviorApplicationModelProvider because it sets parameter binding by its own logic
    /// </remarks>
    public int Order => -900 - 10;

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (var c in context.Result.Controllers)
        {
            if (c.Attributes.OfType<JsonRpcControllerAttribute>().Any())
            {
                foreach (var a in c.Actions)
                {
                    ProcessAction(a);
                    foreach (var p in a.Parameters)
                    {
                        ProcessParameter(p);
                    }
                }
            }
        }
    }

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
    }

    /// <summary>
    /// Configure routing for JSON-RPC endpoints
    /// </summary>
    public void ProcessAction(ActionModel action)
    {
        var selectors = action.Selectors
            .SelectMany(s => CombineRoutes(s, action.Controller))
            .DistinctBy(static s => s.AttributeRouteModel!.Template!.ToLowerInvariant())
            .ToList();
        action.Selectors.Clear();
        foreach (var selector in selectors)
        {
            if (!selector.EndpointMetadata.Any(static m => m is JsonRpcMethodAttribute))
            {
                var method = GetMethodName(action, selector);
                selector.EndpointMetadata.Add(new JsonRpcMethodAttribute(method));
            }

            action.Selectors.Add(selector);
        }
    }

    /// <summary>
    /// Add binding info and metadata to parameters of JSON-RPC endpoints
    /// </summary>
    public void ProcessParameter(ParameterModel parameter)
    {
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
            var propertyName = jsonOptions.Value.JsonSerializerOptions.ConvertName(parameter.ParameterName);
            var parametersMetadata = actionSelector.EndpointMetadata.Get<JsonRpcActionParametersMetadata>();
            if (parametersMetadata == null)
            {
                parametersMetadata = new JsonRpcActionParametersMetadata();
                actionSelector.EndpointMetadata.Add(parametersMetadata);
            }

            parametersMetadata.Parameters[parameter.ParameterName] = new JsonRpcParameterMetadata(propertyName, position, bindingStyle, isOptional, parameter.ParameterName, parameter.ParameterType);
        }
    }

    private string GetMethodName(ActionModel action, SelectorModel selector)
    {
        var jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        var methodStyleAttribute = selector.EndpointMetadata.Get<JsonRpcMethodStyleAttribute>();
        var methodStyle = methodStyleAttribute?.MethodStyle ?? serverOptions.Value.DefaultMethodStyle;

        var controllerName = jsonSerializerOptions.ConvertName(action.Controller.ControllerName);
        var actionName = jsonSerializerOptions.ConvertName(action.ActionName);
        return methodStyle switch
        {
            JsonRpcMethodStyle.ControllerAndAction => $"{controllerName}{JsonRpcConstants.ControllerMethodSeparator}{actionName}",
            JsonRpcMethodStyle.ActionOnly => actionName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static IEnumerable<SelectorModel> CombineRoutes(SelectorModel actionSelector, ControllerModel controller)
    {
        var routeModels = controller.Selectors
            .Select(controllerSelector =>
                AttributeRouteModel.CombineAttributeRouteModel(controllerSelector.AttributeRouteModel, actionSelector.AttributeRouteModel) ?? new AttributeRouteModel())
            .Select(static m => m.Template?.StartsWith('/') == true
                ? m
                : new AttributeRouteModel(m) { Template = $"/{m.Template}" });

        foreach (var combinedRouteModel in routeModels)
        {
            yield return new SelectorModel(actionSelector) { AttributeRouteModel = combinedRouteModel };
        }
    }
}
