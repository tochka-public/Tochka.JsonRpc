using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <inheritdoc />
/// <summary>
/// JSON-RPC id equal to float/double number in JSON
/// </summary>
/// <param name="Value">Actual id value</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record FloatNumberRpcId(double Value) : IRpcId
{
    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
