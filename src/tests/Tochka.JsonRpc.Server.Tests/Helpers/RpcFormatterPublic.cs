using System.Buffers;
using Newtonsoft.Json;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;

namespace Tochka.JsonRpc.Server.Tests.Helpers
{
    public class RpcFormatterPublic : RpcFormatter
    {
        public RpcFormatterPublic(HeaderRpcSerializer headerRpcSerializer, ArrayPool<char> charPool) : base(headerRpcSerializer, charPool)
        {
        }

        public JsonSerializer CreateJsonSerializerPublic()
        {
            return CreateJsonSerializer();
        }
    }
}