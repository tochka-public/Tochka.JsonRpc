using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Tests.Serializers
{
    public class JsonRequests
    {
        public static IEnumerable Cases => Requests.Select(x => new TestCaseData(x.requestString, x.expected));

        private static IEnumerable<(string requestString, object expected)> Requests
        {
            get
            {
                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": null
                    }",
                    new UntypedRequest()
                    {
                        Id = null,
                        Method = "test",
                    }
                );

                yield return (
                    @"{
                        ""id"": ""test"",
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": null
                    }",
                    new UntypedRequest()
                    {
                        Id = new StringRpcId("test"),
                        Method = "test",
                    }
                );

                yield return (
                    @"{
                        ""id"": 1,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": null
                    }",
                    new UntypedRequest()
                    {
                        Id = new NumberRpcId(1),
                        Method = "test",
                    }
                );

                yield return (
                    @"{
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": null
                    }",
                    new UntypedNotification()
                    {
                        Method = "test",
                    }
                );

                yield return (
                    @"[
                        {
                            ""id"": 1,
                            ""jsonrpc"": ""2.0"",
                            ""method"": ""test"",
                            ""params"": null
                        },
                        {
                            ""jsonrpc"": ""2.0"",
                            ""method"": ""test"",
                            ""params"": null
                        },
                    ]",
                    new List<IUntypedCall>()
                    {
                        new UntypedRequest()
                        {
                            Id = new NumberRpcId(1),
                            Method = "test",
                        },
                        new UntypedNotification()
                        {
                            Method = "test",
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": 1,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": []
                    }",
                    new Request<List<object>>
                    {
                        Id = new NumberRpcId(1),
                        Method = "test",
                        Params = new List<object>()
                    }
                );

                yield return (
                    @"{
                        ""id"": 1,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": [1, 2]
                    }",
                    new Request<List<int>>
                    {
                        Id = new NumberRpcId(1),
                        Method = "test",
                        Params = new List<int>
                        {
                            1,
                            2
                        }
                    }
                );

                yield return (
                    @"{
                        ""id"": 1,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": {}
                    }",
                    new Request<Dictionary<int, int>>
                    {
                        Id = new NumberRpcId(1),
                        Method = "test",
                        Params = new Dictionary<int, int>()
                    }
                );

                yield return (
                    @"{
                        ""id"": 1,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": {
                            ""value"": ""test"",
                            ""other_value"": 1,
                        }
                    }",
                    new Request<TestData>
                    {
                        Id = new NumberRpcId(1),
                        Method = "test",
                        Params = new TestData()
                    }
                );
            }
        }
    }
}
