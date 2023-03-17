using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.V1.Common.Models.Response.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class BatchResponseWrapper : IResponseWrapper
    {
        public List<IResponse> Batch { get; set; }
    }
}
