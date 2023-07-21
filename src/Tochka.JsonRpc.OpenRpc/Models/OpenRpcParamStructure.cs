using JetBrains.Annotations;

namespace Tochka.JsonRpc.OpenRpc.Models;

[PublicAPI]
public enum OpenRpcParamStructure
{
    Either,
    ByName,
    ByPosition
}
