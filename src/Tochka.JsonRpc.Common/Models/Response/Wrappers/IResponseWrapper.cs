using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

/// <summary>
/// Base interface to support both single and batch responses
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IResponseWrapper
{
}
