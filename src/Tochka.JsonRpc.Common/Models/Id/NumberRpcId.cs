﻿using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <inheritdoc />
/// <summary>
/// JSON-RPC id equal to integer number in JSON
/// </summary>
/// <param name="Value">Actual id value</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record NumberRpcId
(
    long Value
) : IRpcId
{
    /// <inheritdoc />
    public override string ToString() => $"{Value}";
}
