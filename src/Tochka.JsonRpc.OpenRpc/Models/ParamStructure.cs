using System.Runtime.Serialization;

namespace Tochka.JsonRpc.OpenRpc.Models;

public enum ParamStructure
{
    Either,
    ByName,
    ByPosition
}
