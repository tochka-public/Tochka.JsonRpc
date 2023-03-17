using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.V1.Common.Models.Request.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class BadRequestWrapper : IRequestWrapper
    {
        public Exception Exception { get; set; }
    }
}
