using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class SingleResponseWrapper : IResponseWrapper
    {
        public IResponse Single { get; set; }
    }
}