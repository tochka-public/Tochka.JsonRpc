using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.V1.Server.Binding;
using Tochka.JsonRpc.V1.Server.Settings;

namespace Tochka.JsonRpc.V1.Server.Attributes
{
    /// <summary>
    /// Override binding source for JSON Rpc parameters
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromParamsAttribute : Attribute, IBindingSourceMetadata, IBinderTypeProviderMetadata
    {
        public FromParamsAttribute(BindingStyle bindingStyle)
        {
            BindingStyle = bindingStyle;
        }

        public BindingStyle BindingStyle { get; }

        public BindingSource BindingSource => BindingSource.Custom;
        public Type BinderType => typeof(JsonRpcModelBinder);
    }
}