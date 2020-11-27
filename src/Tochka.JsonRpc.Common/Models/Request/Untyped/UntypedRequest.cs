using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped
{
    [ExcludeFromCodeCoverage]
    public class UntypedRequest : Request<JContainer>, IUntypedCall
    {
        /// <summary>
        /// Set on deserialization. JSON content corresponding to this object
        /// </summary>
        [JsonIgnore]
        public string RawJson { get; set; }

        /// <summary>
        /// Set on deserialization. JSON content corresponding to id property
        /// </summary>
        [JsonIgnore]
        public JValue RawId { get; set; }
    }
}