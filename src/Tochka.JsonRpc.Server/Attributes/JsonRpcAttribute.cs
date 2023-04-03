using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Server.Attributes;

/// <summary>
/// Match JSON Rpc method to action metadata
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class JsonRpcAttribute : ActionMethodSelectorAttribute
{
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
    {
        if (!routeContext.HttpContext.Items.ContainsKey(JsonRpcConstants.NestedPipelineItemKey))
        {
            return false;
        }

        var call = routeContext.HttpContext.GetJsonRpcCall();
        var methodMetadata = action.GetProperty<MethodMetadata>();
        var matcher = routeContext.HttpContext.RequestServices.GetRequiredService<IMethodMatcher>();
        return matcher.IsMatch(methodMetadata, call.Method);
    }
}
