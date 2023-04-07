using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server;

public class JsonRpcActionModelConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        if (!action.Controller.Attributes.Any(static a => a is JsonRpcControllerAttribute))
        {
            return;
        }

        foreach (var actionSelector in action.Selectors)
        {
            AdjustRoutes(actionSelector, action.Controller);

            if (!actionSelector.EndpointMetadata.Any(static m => m is JsonRpcActionAttribute))
            {
                var method = JsonRpcSerializerOptions.Headers.PropertyNamingPolicy!.ConvertName(action.ActionName);
                actionSelector.EndpointMetadata.Add(new JsonRpcActionAttribute(method));
            }
        }
    }

    private void AdjustRoutes(SelectorModel actionSelector, ControllerModel controller)
    {
        var prefix = new PathString(JsonRpcConstants.DefaultRoutePrefix); // TODO get from options
        var routeModels = controller.Selectors
            .Select(controllerSelector =>
                AttributeRouteModel.CombineAttributeRouteModel(controllerSelector.AttributeRouteModel, actionSelector.AttributeRouteModel)
                ?? new AttributeRouteModel { Template = prefix });

        foreach (var combinedRouteModel in routeModels)
        {
            var path = new PathString(combinedRouteModel.Template);
            if (!path.StartsWithSegments(prefix))
            {
                combinedRouteModel.Template = prefix.Add(path);
            }
        }
    }
}
