using System;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Server.Settings
{
    public class JsonRpcOptions
    {
        public JsonRpcOptions()
        {
            HeaderSerializer = typeof(HeaderRpcSerializer);
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