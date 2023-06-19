using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

[ExcludeFromCodeCoverage]
public sealed record StringRpcId(string Value) : IRpcId
{
    public override string ToString() => $"\"{Value}\"";
}
