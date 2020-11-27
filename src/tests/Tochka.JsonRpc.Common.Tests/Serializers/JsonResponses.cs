using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Common.Tests.Serializers
{
    public class JsonResponses
    {
        public static IEnumerable Cases => Responses.Select(x => new TestCaseData(x.responseString, x.expected));

        private static IEnumerable<(string responseString, object expected)> Responses
        {
            get
            {
                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""result"": null,
                    }",
                    new Response<object>()
                    {
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""result"": 1,
                    }",
                    new Response<int>()
                    {
                        Result = 1
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""result"": ""test"",
                    }",
                    new Response<string>()
                    {
                        Result = "test"
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""result"": [1,2,3],
                    }",
                    new Response<List<int>>()
                    {
                        Result = new List<int>
                        {
                            1,
                            2,
                            3
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""result"": {
                            ""value"": ""test"",
                            ""other_value"": 1,
                        }
                    }",
                    new Response<TestData>()
                    {
                        Result = new TestData()
                    }
                );
            }
        }
    }
}
