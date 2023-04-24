using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server;

internal record JsonRpcParameterMetadata(string PropertyName, int Position, BindingStyle BindingStyle, bool IsOptional);
