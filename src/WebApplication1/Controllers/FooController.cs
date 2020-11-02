using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Settings;

namespace WebApplication1.Controllers
{
    public class FooController : JsonRpcController
    {
        private readonly IActionDescriptorProvider actionDescriptorProvider;
        private readonly IApiDescriptionProvider apiDescriptionProvider;
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        private readonly IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider;

        public FooController(IActionDescriptorProvider actionDescriptorProvider, IApiDescriptionProvider apiDescriptionProvider, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider)
        {
            this.actionDescriptorProvider = actionDescriptorProvider;
            this.apiDescriptionProvider = apiDescriptionProvider;
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            this.apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
        }

        public string Simple(int a, string b)
        {
            return $"{a}, {b}";
        }

        public string SimpleObject(TestData testData)
        {
            return $"{testData.Value}, {testData.AnotherValue}";
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

        public string BindMixedObject(int a, [FromParams(BindingStyle.Object)] TestData data)
        {
            return $"{a}, {data.Value}, {data.AnotherValue}";
        }

        public string BindMixedArray(string s, [FromParams(BindingStyle.Array)] List<int> values)
        {
            return $"{s}; {string.Join(", ", values)}";
        }

        private List<int> BindArray([FromParams(BindingStyle.Array)] List<int> values)
        {
            return values;
        }

        public class TestData
        {
            [JsonProperty("testdata_value_JsonProperty")]
            public int Value { get; set; }
            public string AnotherValue { get; set; }
        }
    }
}