using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <inheritdoc />
/// <summary>
/// JSON-RPC id equal to float/double number in JSON
/// </summary>
/// <param name="Value">Actual id value</param>
[ExcludeFromCodeCoverage]
public sealed record FloatNumberRpcId
(
    double Value
) : IRpcId
{
    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
