using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.V1.Common.Models.Response.Errors
{
    [ExcludeFromCodeCoverage]
    public class Error<T> : IError
    {
        public int Code { get; set; }

        // SHOULD be limited to a concise single sentence.
        public string Message { get; set; }

        // This may be omitted
        public T Data { get; set; }

        public object GetData()
        {
            return Data;
        }
    }
}
