using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Routing;

/// <inheritdoc />
/// <summary>
/// <see cref="IActionModelConvention" /> to configure routing for JSON-RPC endpoints
/// </summary>
internal class JsonRpcActionModelConvention : IActionModelConvention
{
    private readonly JsonOptions options;
    private readonly JsonRpcServerOptions serverOptions;

    public JsonRpcActionModelConvention(IOptions<JsonOptions> options, IOptions<JsonRpcServerOptions> serverOptions)
    {
        this.options = options.Value;
        this.serverOptions = serverOptions.Value;
    }

    public void Apply(ActionModel action)
    {
        if (!action.Controller.Attributes.Any(static a => a is JsonRpcControllerAttribute))
        {
            return;
        }

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

    private IEnumerable<SelectorModel> CombineRoutes(SelectorModel actionSelector, ControllerModel controller)
    {
        var routePrefix = serverOptions.RoutePrefix;
        var routeModels = controller.Selectors
            .Select(controllerSelector =>
                AttributeRouteModel.CombineAttributeRouteModel(controllerSelector.AttributeRouteModel, actionSelector.AttributeRouteModel)
                ?? new AttributeRouteModel { Template = routePrefix.Value })
            .Select(static m => m.Template!.StartsWith('/')
                ? m
                : new AttributeRouteModel(m) { Template = $"/{m.Template}" });

        foreach (var combinedRouteModel in routeModels)
        {
            var path = new PathString(combinedRouteModel.Template);
            if (!path.StartsWithSegments(routePrefix))
            {
                combinedRouteModel.Template = routePrefix.Add(path).Value;
            }

            yield return new SelectorModel(actionSelector) { AttributeRouteModel = combinedRouteModel };
        }
    }

    private string GetMethodName(ActionModel action, SelectorModel selector)
    {
        var jsonSerializerOptions = options.JsonSerializerOptions;
        var methodStyleAttribute = selector.EndpointMetadata.Get<JsonRpcMethodStyleAttribute>();
        var methodStyle = methodStyleAttribute?.MethodStyle ?? serverOptions.DefaultMethodStyle;

        var controllerName = jsonSerializerOptions.ConvertName(action.Controller.ControllerName);
        var actionName = jsonSerializerOptions.ConvertName(action.ActionName);
        return methodStyle switch
        {
            JsonRpcMethodStyle.ControllerAndAction => $"{controllerName}{JsonRpcConstants.ControllerMethodSeparator}{actionName}",
            JsonRpcMethodStyle.ActionOnly => actionName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
