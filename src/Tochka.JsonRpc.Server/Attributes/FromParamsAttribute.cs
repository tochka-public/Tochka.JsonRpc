using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Attributes;

/// <inheritdoc cref="IBinderTypeProviderMetadata" />
/// <summary>
/// Attribute to override default parameter binding behavior
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromParamsAttribute : Attribute, IBinderTypeProviderMetadata
{
    /// <summary>
    /// Style in which JSON-RPC params will be bound to parameter
    /// </summary>
    public BindingStyle BindingStyle { get; }

    public BindingSource BindingSource => BindingSource.Custom;

    public Type BinderType => typeof(JsonRpcModelBinder);

    /// <inheritdoc />
    /// <param name="bindingStyle">Style in which JSON-RPC params will be bound to parameter</param>
    public FromParamsAttribute(BindingStyle bindingStyle) => BindingStyle = bindingStyle;
}
