using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.OpenRpc
{
    internal static class Extensions
    {
        internal static bool IsObsoleteTransitive(this ApiDescription description)
        {
            var method = (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
            var methodAttr = method?.GetCustomAttribute<ObsoleteAttribute>();
            var typeAttr = method?.DeclaringType?.GetCustomAttribute<ObsoleteAttribute>();
            return (methodAttr ?? typeAttr) != null;

        }

        internal static MethodObjectParamStructure ToParamStructure(this BindingStyle bindingStyle)
        {
            switch (bindingStyle)
            {
                case BindingStyle.Default:
                    return MethodObjectParamStructure.Either;
                case BindingStyle.Object:
                    return MethodObjectParamStructure.ByName;
                case BindingStyle.Array:
                    return MethodObjectParamStructure.ByPosition;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bindingStyle), bindingStyle, null);
            }
        }
    }
}