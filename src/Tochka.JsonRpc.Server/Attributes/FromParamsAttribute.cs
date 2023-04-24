using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromParamsAttribute : Attribute, IBinderTypeProviderMetadata
{
    public FromParamsAttribute(BindingStyle bindingStyle) => BindingStyle = bindingStyle;

    public BindingStyle BindingStyle { get; }

    public BindingSource BindingSource => BindingSource.Custom;
    public Type BinderType => typeof(JsonRpcModelBinder);
}
