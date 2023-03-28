using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers;

[ExcludeFromCodeCoverage]
public class BatchResponseWrapper : IResponseWrapper
{
    public List<IResponse> Batch { get; set; }
}
