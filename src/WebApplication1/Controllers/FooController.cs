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
        /// <summary>
        /// Simple.Summary~
        /// </summary>
        /// <param name="firstArg">Simple.Param1~</param>
        /// <param name="secondArg">Simple.Param2~</param>
        /// <returns>Simple.Returns~</returns>
        /// <remarks>Simple.Remarks~</remarks>
        /// <example>Simple.Example~</example>
        /// <response code="222">Simple.Response222~</response>
        /// <response code="200">Values2.Response200~</response>
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


        public string SimpleWithDefaults(int? firstArg, string secondArg="defaultValue", long?thirdArg=null)
        {
            return $"{firstArg}, {secondArg} {thirdArg}";
        }

        public string SimpleObjectNullable(TestData testDataA, TestData testDataB=null)
        {
            return $"{testDataA.Value}, {testDataA.AnotherValue}; {testDataB?.Value}, {testDataB?.AnotherValue}";
        }

        

        /*
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
        */

        /// <summary>
        /// TestData.Summary~
        /// </summary>
        /// <remarks>TestData.Remarks~</remarks>
        /// <example>TestData.Example~</example>
        public class TestData
        {
            /// <summary>
            /// TestData.Value.Summary~
            /// </summary>
            /// <remarks>TestData.Value.Remarks~</remarks>
            /// <example>99999~</example>
            public int Value { get; set; } = 999;
            public string AnotherValue { get; set; } = "defaultAnotherValue";
            public NestedClass Prop1 { get; set; }
            public NestedClass Prop2 { get; set; }
        }

        public class NestedClass
        {
            public string WtfWtf { get; set; }
        }
    }
}