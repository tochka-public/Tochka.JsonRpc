using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class SingleRequestWrapper : IRequestWrapper
    {
        public IUntypedCall Call { get; set; }
    }
}