using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.V1.Common.Models.Request.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class BatchRequestWrapper : IRequestWrapper
    {
        public List<IUntypedCall> Batch { get; set; }
    }
}
