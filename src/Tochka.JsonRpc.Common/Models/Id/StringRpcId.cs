namespace Tochka.JsonRpc.Common.Models.Id;

public record StringRpcId(string Value) : IRpcId
{
    public override string ToString() => $"\"{Value}\"";
}
