using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

/// <summary>
/// Base interface to support both single and batch responses
/// </summary>
[PublicAPI]
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IResponseWrapper
{
}
