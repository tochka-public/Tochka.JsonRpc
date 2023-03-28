using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IResponseWrapper
{
}
