using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id;

[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IRpcId
{
}
