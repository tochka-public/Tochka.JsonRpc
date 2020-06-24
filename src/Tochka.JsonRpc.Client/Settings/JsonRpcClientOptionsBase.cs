using System;

namespace Tochka.JsonRpc.Client.Settings
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
    }

}