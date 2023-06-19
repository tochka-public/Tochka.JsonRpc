using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

[ExcludeFromCodeCoverage]
public sealed record NullRpcId : IRpcId
{
    public override string ToString() => "null";
}
