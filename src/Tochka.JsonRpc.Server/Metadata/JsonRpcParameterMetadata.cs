using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Metadata;

/// <summary>
/// Metadata with information about action parameter
/// </summary>
/// <param name="PropertyName">Expected JSON property name</param>
/// <param name="Position">Expected position in array `params`</param>
/// <param name="BindingStyle"><see cref="BindingStyle" /> for this parameter</param>
/// <param name="IsOptional">Can this parameter be omitted entirely</param>
/// <param name="OriginalName">Original action argument name</param>
/// <param name="Type"><see cref="Type" /> of action argument</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record JsonRpcParameterMetadata(string PropertyName, int Position, BindingStyle BindingStyle, bool IsOptional, string OriginalName, Type Type);
