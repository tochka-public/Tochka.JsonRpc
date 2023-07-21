using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.V1.Common.Models.Request.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class SingleRequestWrapper : IRequestWrapper
    {
        public IUntypedCall Call { get; set; }
    }
}
