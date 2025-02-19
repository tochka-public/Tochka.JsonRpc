using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <inheritdoc />
/// <summary>
/// JSON-RPC id equal to integer number in JSON
/// </summary>
/// <param name="Value">Actual id value</param>
[ExcludeFromCodeCoverage]
public sealed record NumberRpcId
(
    long Value
) : IRpcId
{
    /// <inheritdoc />
    public override string ToString() => $"{Value}";
}
