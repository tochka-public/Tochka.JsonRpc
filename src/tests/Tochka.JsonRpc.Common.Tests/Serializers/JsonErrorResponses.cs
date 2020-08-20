using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Common.Tests.Serializers
{
    public class JsonErrorResponses
    {
        public static IEnumerable Cases => ErrorResponses.Select(x => new TestCaseData(x.responseString, x.expected));

        private static IEnumerable<(string responseString, object expected)> ErrorResponses
        {
            get
            {
                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""error"": null,
                    }",
                    new ErrorResponse<object>()
                    {
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""error"": {
                            ""code"": -32001,
                            ""message"": ""Server error"",
                        }
                    }",
                    new UntypedErrorResponse()
                    {
                        Error = new Error<JToken>
                        {
                            Code = -32001,
                            Message = "Server error",
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""error"": {
                            ""code"": -32001,
                            ""message"": ""Server error"",
                            ""data"": ""test""
                        }
                    }",
                    new ErrorResponse<string>()
                    {
                        Error = new Error<string>
                        {
                            Code = -32001,
                            Message = "Server error",
                            Data = "test"
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""error"": {
                            ""code"": -32001,
                            ""message"": ""Server error"",
                            ""data"": 1
                        }
                    }",
                    new UntypedErrorResponse()
                    {
                        Error = new Error<JToken>()
                        {
                            Code = -32001,
                            Message = "Server error",
                            Data = new JValue(1)
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""error"": {
                            ""code"": -32001,
                            ""message"": ""Server error"",
                            ""data"": {
                                ""value"": ""test"",
                                ""other_value"": 1,
                            }
                        }
                    }",
                    new UntypedErrorResponse()
                    {
                        Error = new Error<JToken>()
                        {
                            Code = -32001,
                            Message = "Server error",
                            Data = new JObject()
                            {
                                {"value", "test"},
                                {"other_value", 1},
                            }
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""error"": {
                            ""code"": -32001,
                            ""message"": ""Server error"",
                            ""data"": {
                                ""value"": ""test"",
                                ""other_value"": 1,
                            }
                        }
                    }",
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>()
                        {
                            Code = -32001,
                            Message = "Server error",
                            Data = new TestData()
                            
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""error"": {
                            ""code"": -32001,
                            ""message"": ""Server error""
                        }
                    }",
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>()
                        {
                            Code = -32001,
                            Message = "Server error",
                            Data = null

                        }
                    }
                );
            }
        }
        /*"id": null,
         "jsonrpc": "2.0",
         "error": {
           "code": -32001,
           "message": "Server error",
           "data": null
         }
*/

    }
}