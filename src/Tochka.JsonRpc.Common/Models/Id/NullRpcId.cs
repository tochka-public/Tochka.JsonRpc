using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <inheritdoc />
/// <summary>
/// JSON-RPC id equal to `null` in JSON
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record NullRpcId : IRpcId
{
    public override string ToString() => "null";
}
