namespace Tochka.JsonRpc.Common.Models.Id;

public record NullRpcId : IRpcId
{
    public override string ToString() => "null";
}
