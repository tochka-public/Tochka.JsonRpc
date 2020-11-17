using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Infrastructure;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Settings;

namespace WebApplication1.Controllers
{
    public class FooController : JsonRpcController
    {
        public string Simple(int firstArg, string secondArg)
        {
            return $"{firstArg}, {secondArg}";
        }

        public string SimpleObject(TestData testDataA, TestData testDataB)
        {
            return $"{testDataA.Value}, {testDataA.AnotherValue}; {testDataB.Value}, {testDataB.AnotherValue}";
        }

        public TestData BindObject([FromParams(BindingStyle.Object)] TestData data)
        {
            return data;
        }

        [JsonRpcSerializer(typeof(SnakeCaseJsonRpcSerializer))]
        public TestData BindObjectSnake([FromParams(BindingStyle.Object)] TestData data)
        {
            return data;
        }

        [JsonRpcSerializer(typeof(CamelCaseJsonRpcSerializer))]
        public TestData BindObjectCamel([FromParams(BindingStyle.Object)] TestData data)
        {
            return data;
        }

        [Route("/api/v2")]
        public string BindMixedObject(int a, [FromParams(BindingStyle.Object)] TestData data)
        {
            return $"{a}, {data.Value}, {data.AnotherValue}";
        }

        public string BindMixedArray(string s, [FromParams(BindingStyle.Array)] List<int> values)
        {
            return $"{s}; {string.Join(", ", values)}";
        }

        public List<int> BindArray([FromParams(BindingStyle.Array)] List<int> values)
        {
            return values;
        }

        public class TestData
        {
            
            public int Value { get; set; }
            public string AnotherValue { get; set; }
            public NestedClass Prop1 { get; set; }
            public NestedClass Prop2 { get; set; }
        }

        public class NestedClass
        {
            public string WtfWtf { get; set; }
        }
    }
}