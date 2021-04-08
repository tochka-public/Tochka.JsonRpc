using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Common.Models.Response.Untyped
{
    [ExcludeFromCodeCoverage]
    public class UntypedResponse : Response<JToken>
    {
        /// <summary>
        /// Set on deserialization. JSON content corresponding to result property
        /// </summary>
        [JsonIgnore]
        public string RawResult { get; set; }

        /// <summary>
        /// Set on deserialization. JSON content corresponding to id property
        /// </summary>
        [JsonIgnore]
        public JValue RawId { get; set; }
    }
}