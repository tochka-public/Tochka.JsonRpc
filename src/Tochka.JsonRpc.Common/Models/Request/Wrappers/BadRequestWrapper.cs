using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers;

[ExcludeFromCodeCoverage]
public class BadRequestWrapper : IRequestWrapper
{
    public Exception Exception { get; set; }
}
