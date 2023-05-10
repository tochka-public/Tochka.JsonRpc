using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Routing;

internal class JsonRpcActionModelConvention : IActionModelConvention
{
    private readonly IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private readonly JsonRpcServerOptions options;

    public JsonRpcActionModelConvention(IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders, IOptions<JsonRpcServerOptions> options)
    {
        this.serializerOptionsProviders = serializerOptionsProviders;
        this.options = options.Value;
    }

    public void Apply(ActionModel action)
    {
        if (!action.Controller.Attributes.Any(static a => a is JsonRpcControllerAttribute))
        {
            return;
        }

        var selectors = action.Selectors.SelectMany(s => CombineRoutes(s, action.Controller)).ToArray();
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
        var routeModels = controller.Selectors
            .Select(controllerSelector =>
                AttributeRouteModel.CombineAttributeRouteModel(controllerSelector.AttributeRouteModel, actionSelector.AttributeRouteModel)
                ?? new AttributeRouteModel { Template = options.RoutePrefix })
            .Select(static m => m.Template!.StartsWith('/')
                ? m
                : new AttributeRouteModel(m) { Template = $"/{m.Template}" })
            .DistinctBy(static rm => rm.Template!.ToLowerInvariant());

        foreach (var combinedRouteModel in routeModels)
        {
            var path = new PathString(combinedRouteModel.Template);
            if (!path.StartsWithSegments(options.RoutePrefix))
            {
                combinedRouteModel.Template = options.RoutePrefix.Add(path).Value;
            }

            yield return new SelectorModel(actionSelector) { AttributeRouteModel = combinedRouteModel };
        }
    }

    private string GetMethodName(ActionModel action, SelectorModel selector)
    {
        var jsonSerializerOptions = Utils.GetDataJsonSerializerOptions(selector.EndpointMetadata, options, serializerOptionsProviders);
        var methodStyleMetadata = selector.EndpointMetadata.FirstOrDefault(static m => m is JsonRpcMethodStyleAttribute);
        var methodStyle = methodStyleMetadata is JsonRpcMethodStyleAttribute methodStyleAttribute
            ? methodStyleAttribute.MethodStyle
            : options.DefaultMethodStyle;

        var controllerName = jsonSerializerOptions.PropertyNamingPolicy!.ConvertName(action.Controller.ControllerName);
        var actionName = jsonSerializerOptions.PropertyNamingPolicy.ConvertName(action.ActionName);
        return methodStyle switch
        {
            JsonRpcMethodStyle.ControllerAndAction => $"{controllerName}.{actionName}",
            JsonRpcMethodStyle.ActionOnly => actionName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
