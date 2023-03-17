using System;

namespace Tochka.JsonRpc.V1.Client.Settings
{
    /// <summary>
    /// Base class for JSON Rpc client with sane default values
    /// </summary>
    public abstract class JsonRpcClientOptionsBase
    {
        /// <summary>
        /// HTTP endpoint
        /// </summary>
        public virtual string Url { get; set; }

        /// <summary>
        /// Request timeout, default is 10 seconds
        /// </summary>
        public virtual TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// If true, logs requests/notifications/batches
        /// </summary>
        public bool LogRequests { get; set; }
    }

}
