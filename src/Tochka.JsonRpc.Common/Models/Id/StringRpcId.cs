namespace Tochka.JsonRpc.Common.Models.Id;

public sealed record StringRpcId(string Value) : IRpcId
{
    public override string ToString() => $"\"{Value}\"";
}
