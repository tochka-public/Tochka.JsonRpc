using Tochka.JsonRpc.Client.Settings;

namespace Tochka.JsonRpc.Client.Tochka
{
    public abstract class TochkaJsonRpcClientOptionsBase : JsonRpcClientOptionsBase
    {
        /// <summary>
        /// Http header name for authentication key
        /// </summary>
        public string AuthenticationHeader { get; set; } = TochkaConstants.DefaultAuthenticationHeader;

        /// <summary>
        /// Authentication key
        /// </summary>
        public string AuthenticationKey { get; set; }
    }
}
