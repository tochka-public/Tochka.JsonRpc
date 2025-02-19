using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <inheritdoc />
/// <summary>
/// JSON-RPC id equal to `null` in JSON
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record NullRpcId : IRpcId
{
    /// <inheritdoc />
    public override string ToString() => "null";
}
