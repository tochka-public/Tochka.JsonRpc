using System.Runtime.Serialization;

namespace Tochka.JsonRpc.OpenRpc.Models;

public enum ParamStructure
{
    [EnumMember(Value = "either")]
    Either,

    [EnumMember(Value = "by-name")]
    ByName,

    [EnumMember(Value = "by-position")]
    ByPosition
}
