using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.IntegrationTests.Models;

namespace Tochka.JsonRpc.Server.IntegrationTests.Cases
{
    /// <summary>
    /// Test default internal HTTP code with returned object handling
    /// </summary>
    public class HttpObjectCodeRequests
    {
        public static IEnumerable Cases => Requests.Select(x => new TestCaseData(x.data, x.expected));

        private static IEnumerable<(object data, object expected)> Requests
        {
            get
            {
                foreach (var x in Enumerable.Range(0, 100))
                {
                    yield return (
                        CreateRequest(x),
                        new ErrorResponse<TestData>()
                        {
                            Error = new Error<TestData>
                            {
                                Code = JsonRpcConstants.InternalExceptionCode,
                                Message = "Server error",
                                Data = new TestData()
                            }
                        });
                }

                foreach (var x in Enumerable.Range(200, 100))
                {
                    yield return (
                        CreateRequest(x),
                        new Response<TestData>()
                        {
                            Result = new TestData()
                        });
                }
                var special = new HashSet<int>() { 400, 422, 401, 403, 404, 415, 500 };
                var range = Enumerable.Range(300, 300).Except(special);
                foreach (var x in range)
                {
                    yield return (
                        CreateRequest(x),
                        new ErrorResponse<TestData>()
                        {
                            Error = new Error<TestData>
                            {
                                Code = JsonRpcConstants.InternalExceptionCode,
                                Message = "Server error",
                                Data = new TestData()
                            }
                        });
                }
                yield return (
                    CreateRequest(400),
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>
                        {
                            Code = -32602,
                            Message = "Invalid params",
                            Data = new TestData()
                        }
                    });

                yield return (
                    CreateRequest(422),
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>
                        {
                            Code = -32602,
                            Message = "Invalid params",
                            Data = new TestData()
                        }
                    });

                yield return (
                    CreateRequest(401),
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>
                        {
                            Code = -32601,
                            Message = "Method not found",
                            Data = new TestData()
                        }
                    });

                yield return (
                    CreateRequest(403),
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>
                        {
                            Code = -32601,
                            Message = "Method not found",
                            Data = new TestData()
                        }
                    });

                yield return (
                    CreateRequest(404),
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>
                        {
                            Code = -32004,
                            Message = "Not found",
                            Data = new TestData()
                        }
                    });

                yield return (
                    CreateRequest(415),
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>
                        {
                            Code = -32700,
                            Message = "Parse error",
                            Data = new TestData()
                        }
                    });

                yield return (
                    CreateRequest(500),
                    new ErrorResponse<TestData>()
                    {
                        Error = new Error<TestData>
                        {
                            Code = -32603,
                            Message = "Internal error",
                            Data = new TestData()
                        }
                    });

            }
        }

        private static Request<object> CreateRequest(int x)
        {
            return new Request<object>()
            {
                Method = "params.http_object_code",
                Params = new
                {
                    statusCode = x,
                    value = new TestData()
                }
            };
        }
    }
}