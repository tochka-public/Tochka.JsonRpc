using System.Runtime.Serialization;

namespace Tochka.JsonRpc.V1.OpenRpc.Models
{
    public enum MethodObjectParamStructure
    {
        [EnumMember(Value = "either")]
        Either,
        [EnumMember(Value = "by-name")]
        ByName,
        [EnumMember(Value = "by-position")]
        ByPosition
    }
}