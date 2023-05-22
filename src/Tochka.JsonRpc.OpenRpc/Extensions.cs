using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.OpenRpc;

public static class Extensions
{
    internal static bool IsObsoleteTransitive(this ApiDescription description)
    {
        var methodInfo = (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
        var methodAttr = methodInfo?.GetCustomAttribute<ObsoleteAttribute>();
        var typeAttr = methodInfo?.DeclaringType?.GetCustomAttribute<ObsoleteAttribute>();
        return (methodAttr ?? typeAttr) != null;
    }

    internal static ParamStructure ToParamStructure(this BindingStyle bindingStyle) => bindingStyle switch
    {
        BindingStyle.Default => ParamStructure.Either,
        BindingStyle.Object => ParamStructure.ByName,
        BindingStyle.Array => ParamStructure.ByPosition,
        _ => throw new ArgumentOutOfRangeException(nameof(bindingStyle), bindingStyle, null)
    };
}
