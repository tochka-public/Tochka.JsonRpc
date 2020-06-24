using System.Buffers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Server.Binding
{
    public class RpcFormatter : JsonOutputFormatter
    {
        private readonly HeaderRpcSerializer headerRpcSerializer;

        public RpcFormatter(HeaderRpcSerializer headerRpcSerializer, ArrayPool<char> charPool) : base(headerRpcSerializer.Settings, charPool)
        {
            this.headerRpcSerializer = headerRpcSerializer;
        }

        protected override JsonSerializer CreateJsonSerializer()
        {
            return headerRpcSerializer.Serializer;
        }
    }
}