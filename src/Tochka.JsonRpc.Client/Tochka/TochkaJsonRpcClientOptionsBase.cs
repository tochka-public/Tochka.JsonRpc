using Tochka.JsonRpc.Client.Settings;

namespace Tochka.JsonRpc.Client.Tochka
{
    public abstract class TochkaJsonRpcClientOptionsBase : JsonRpcClientOptionsBase
    {
        public string AuthenticationHeader { get; set; } = TochkaConstants.DefaultAuthenticationHeader;
        public string AuthenticationKey { get; set; }
    }
}
