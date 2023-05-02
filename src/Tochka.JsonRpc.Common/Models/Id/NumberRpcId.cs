namespace Tochka.JsonRpc.Common.Models.Id;

public sealed record NumberRpcId(long Value) : IRpcId
{
    public override string ToString() => $"{Value}";
}
