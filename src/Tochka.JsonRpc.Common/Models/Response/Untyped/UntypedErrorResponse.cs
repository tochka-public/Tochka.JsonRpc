using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Common.Models.Response.Untyped
{
    [ExcludeFromCodeCoverage]
    public class UntypedErrorResponse : ErrorResponse<JToken>
    {
        /// <summary>
        /// Set on deserialization. JSON content corresponding to error property
        /// </summary>
        [JsonIgnore]
        public string RawError { get; set; }

        /// <summary>
        /// Set on deserialization. JSON content corresponding to id property
        /// </summary>
        [JsonIgnore]
        public JValue RawId { get; set; }
    }
}