using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

/// <summary>
/// Base interface to support all JSON-RPC id types
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IRpcId
{
}
