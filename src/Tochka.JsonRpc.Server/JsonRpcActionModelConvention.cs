using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server;

public class JsonRpcActionModelConvention : IActionModelConvention
{
    private readonly JsonRpcServerOptions options;

    public JsonRpcActionModelConvention(IOptions<JsonRpcServerOptions> options) => this.options = options.Value;

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
                var method = JsonRpcSerializerOptions.Headers.PropertyNamingPolicy!.ConvertName(action.ActionName);
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
}
