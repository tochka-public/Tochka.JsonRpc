using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <summary>
/// Base interface to support all JSON-RPC id types
/// </summary>
[PublicAPI]
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IRpcId
{
}
