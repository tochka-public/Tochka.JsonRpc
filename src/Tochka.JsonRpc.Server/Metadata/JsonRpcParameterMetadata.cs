using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Metadata;

public sealed record JsonRpcParameterMetadata(string PropertyName, int Position, BindingStyle BindingStyle, bool IsOptional);
