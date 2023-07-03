using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Metadata;

[ExcludeFromCodeCoverage]
public sealed record JsonRpcParameterMetadata(string PropertyName, int Position, BindingStyle BindingStyle, bool IsOptional, string OriginalName, Type Type);
