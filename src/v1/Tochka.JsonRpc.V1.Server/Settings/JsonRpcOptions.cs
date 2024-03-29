using System;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Server.Settings
{
    public class JsonRpcOptions
    {
        public JsonRpcOptions()
        {
            HeaderSerializer = typeof(HeaderJsonRpcSerializer);
            AllowRawResponses = false;
            DetailedResponseExceptions = false;
            DefaultMethodOptions = new JsonRpcMethodOptions();
            BatchHandling = BatchHandling.Sequential;
        }

        public Type HeaderSerializer { get; set; }

        public bool AllowRawResponses { get; set; }

        public bool DetailedResponseExceptions { get; set; }

        public JsonRpcMethodOptions DefaultMethodOptions { get; set; }

        public BatchHandling BatchHandling { get; set; }

        //public int BatchConcurrencyLimit { get; set; }
    }
}
