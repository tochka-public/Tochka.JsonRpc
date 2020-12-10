using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Common.Models.Request.Untyped
{
    [ExcludeFromCodeCoverage]
    public class UntypedNotification : Notification<JContainer>, IUntypedCall
    {
        /// <summary>
        /// Set on deserialization. JSON content corresponding to this object
        /// </summary>
        public string RawJson { get; set; }
    }
}