using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.IntegrationTests.Models;

namespace Tochka.JsonRpc.Server.IntegrationTests.Cases
{
    /// <summary>
    /// Test bad json
    /// </summary>
    public class BadRequests
    {
        public static IEnumerable Cases => Requests.Select(x => new TestCaseData(x.data, x.expected));

        private static IEnumerable<(string data, object expected)> Requests
        {
            get
            {
                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""test"",
                        ""params"": nulll
                    }",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32700,
                            Message = "Parse error",
                        }
                    });

                yield return (
                    @"{",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32700,
                            Message = "Parse error",
                        }
                    });

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""1.0"",
                        ""method"": ""test"",
                        ""params"": null
                    }",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32600,
                            Message = "Invalid Request",
                        }
                    });

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""method"": """",
                        ""params"": null
                    }",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32600,
                            Message = "Invalid Request",
                        }
                    });

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""method"": null,
                        ""params"": null
                    }",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32600,
                            Message = "Invalid Request",
                        }
                    });

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""params"": null
                    }",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32600,
                            Message = "Invalid Request",
                        }
                    });

                yield return (
                    @"{
                        ""id"": 1.1,
                        ""jsonrpc"": ""2.0"",
                        ""params"": null
                    }",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32600,
                            Message = "Invalid Request",
                        }
                    });

                yield return (
                    @"{
                        ""id"": null,
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""params.http_code"",
                        ""params"": null
                    }",
                    new ErrorResponse<IgnoreData>()
                    {
                        Error = new Error<IgnoreData>
                        {
                            Code = -32602,
                            Message = "Invalid params",
                        }
                    });

                yield return (
                    @"[]",
                    new ErrorResponse<ExceptionInfo>()
                    {
                        Error = new Error<ExceptionInfo>
                        {
                            Code = -32001,
                            Message = "Server error",
                            Data = new ExceptionInfo()
                            {
                                Message = "JSON Rpc batch request is empty",
                                Type = "Tochka.JsonRpc.Server.Exceptions.JsonRpcInternalException"
                            }
                        }
                    });
            }
        }
    }
}