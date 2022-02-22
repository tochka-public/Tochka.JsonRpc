using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;

namespace Tochka.JsonRpc.Server.Tests.Helpers
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