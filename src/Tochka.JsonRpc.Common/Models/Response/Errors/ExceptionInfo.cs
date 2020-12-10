using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Response.Errors
{
    /// <summary>
    /// Server-defined details about exceptions and unexpected HTTP codes
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ExceptionInfo
    {
        public int? InternalHttpCode { get; set; }
        public string Message { get; set; }
        public object Details { get; set; }
        public string Type { get; set; }
    }
}