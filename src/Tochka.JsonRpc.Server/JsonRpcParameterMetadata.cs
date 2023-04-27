using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server;

public sealed record JsonRpcParameterMetadata(string PropertyName, int Position, BindingStyle BindingStyle, bool IsOptional);
