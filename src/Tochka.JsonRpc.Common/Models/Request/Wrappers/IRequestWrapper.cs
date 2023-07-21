using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

/// <summary>
/// Base interface to support both single and batch requests
/// </summary>
[PublicAPI]
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Required for serialization")]
public interface IRequestWrapper
{
}
