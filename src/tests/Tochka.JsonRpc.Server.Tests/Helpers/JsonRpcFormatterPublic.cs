using System.Buffers;
using Newtonsoft.Json;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;

namespace Tochka.JsonRpc.Server.Tests.Helpers
{
    public class JsonRpcFormatterPublic : JsonRpcFormatter
    {
        public JsonRpcFormatterPublic(HeaderJsonRpcSerializer headerJsonRpcSerializer, ArrayPool<char> charPool) : base(headerJsonRpcSerializer, charPool)
        {
        }

        public JsonSerializer CreateJsonSerializerPublic()
        {
            return CreateJsonSerializer();
        }
    }
}