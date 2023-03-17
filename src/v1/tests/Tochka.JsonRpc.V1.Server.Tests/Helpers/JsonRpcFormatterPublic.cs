using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server.Binding;

namespace Tochka.JsonRpc.V1.Server.Tests.Helpers
{
    public class JsonRpcFormatterPublic : JsonRpcFormatter
    {
        public JsonRpcFormatterPublic(HeaderJsonRpcSerializer headerJsonRpcSerializer, ArrayPool<char> charPool, IOptions<MvcOptions> mvcOptions, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions) : base(headerJsonRpcSerializer, charPool, mvcOptions, mvcNewtonsoftJsonOptions)
        {
        }

        public JsonSerializer CreateJsonSerializerPublic()
        {
            return CreateJsonSerializer();
        }
    }
}
