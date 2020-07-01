using System.Buffers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Server.Binding
{
    public class JsonRpcFormatter : JsonOutputFormatter
    {
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;

        public JsonRpcFormatter(HeaderJsonRpcSerializer headerJsonRpcSerializer, ArrayPool<char> charPool) : base(headerJsonRpcSerializer.Settings, charPool)
        {
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
        }

        protected override JsonSerializer CreateJsonSerializer()
        {
            return headerJsonRpcSerializer.Serializer;
        }
    }
}