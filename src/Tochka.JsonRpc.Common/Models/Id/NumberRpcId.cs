namespace Tochka.JsonRpc.Common.Models.Id;

public record NumberRpcId(long Value) : IRpcId
{
    public override string ToString() => $"{Value}";
}
