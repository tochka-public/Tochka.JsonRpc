using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IRequestWrapper
{
}
