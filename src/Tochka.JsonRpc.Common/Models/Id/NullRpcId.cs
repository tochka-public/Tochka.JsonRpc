namespace Tochka.JsonRpc.Common.Models.Id;

public sealed record NullRpcId : IRpcId
{
    public override string ToString() => "null";
}
