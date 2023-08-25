using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <inheritdoc />
/// <summary>
/// JSON-RPC id equal to string in JSON
/// </summary>
/// <param name="Value">Actual id value</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record StringRpcId(string Value) : IRpcId
{
    /// <inheritdoc />
    public override string ToString() => $"\"{Value}\"";
}
