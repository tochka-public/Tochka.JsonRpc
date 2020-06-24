using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Server.IntegrationTests.Cases
{
    /// <summary>
    /// Test default internal HTTP code handling
    /// </summary>
    public class HttpCodeRequests
    {
        public static IEnumerable Cases => Requests.Select(x => new TestCaseData(x.data, x.expected));

        private static IEnumerable<(object data, object expected)> Requests
        {
            get
            {
                foreach (var x in Enumerable.Range(0, 100))
                {
                    yield return (
                        new Request<object>()
                        {
                            Method = "params.http_code",
                            Params = new {statusCode = x}
                        },
                        new ErrorResponse<object>()
                        {
                            Error = new Error<object>
                            {
                                Code = JsonRpcConstants.InternalExceptionCode,
                                Message = "Server error",
                            }
                        });
                }
                foreach (var x in Enumerable.Range(200, 100))
                {
                    yield return (
                        new Request<object>()
                        {
                            Method = "params.http_code",
                            Params = new { statusCode= x }
                        },
                        new Response<object>());
                }

                var special = new HashSet<int>(){400, 422, 401, 403, 404, 415, 500};
                var range = Enumerable.Range(300, 300).Except(special);
                foreach (var x in range)
                {
                    yield return (
                        new Request<object>()
                        {
                            Method = "params.http_code",
                            Params = new { statusCode = x }
                        },
                        new ErrorResponse<object>()
                        {
                            Error = new Error<object>
                            {
                                Code = JsonRpcConstants.InternalExceptionCode,
                                Message = "Server error",
                            }
                        });
                }

                yield return (
                    new Request<object>()
                    {
                        Method = "params.http_code",
                        Params = new {statusCode = 400}
                    },
                    new ErrorResponse<object>()
                    {
                        Error = new Error<object>
                        {
                            Code = -32602,
                            Message = "Invalid params",
                        }
                    });

                yield return (
                    new Request<object>()
                    {
                        Method = "params.http_code",
                        Params = new { statusCode = 422 }
                    },
                    new ErrorResponse<object>()
                    {
                        Error = new Error<object>
                        {
                            Code = -32602,
                            Message = "Invalid params",
                        }
                    });

                yield return (
                    new Request<object>()
                    {
                        Method = "params.http_code",
                        Params = new { statusCode = 401 }
                    },
                    new ErrorResponse<object>()
                    {
                        Error = new Error<object>
                        {
                            Code = -32601,
                            Message = "Method not found",
                        }
                    });

                yield return (
                    new Request<object>()
                    {
                        Method = "params.http_code",
                        Params = new { statusCode = 403 }
                    },
                    new ErrorResponse<object>()
                    {
                        Error = new Error<object>
                        {
                            Code = -32601,
                            Message = "Method not found",
                        }
                    });

                yield return (
                    new Request<object>()
                    {
                        Method = "params.http_code",
                        Params = new { statusCode = 404 }
                    },
                    new ErrorResponse<object>()
                    {
                        Error = new Error<object>
                        {
                            Code = -32004,
                            Message = "Not found",
                        }
                    });

                yield return (
                    new Request<object>()
                    {
                        Method = "params.http_code",
                        Params = new { statusCode = 415 }
                    },
                    new ErrorResponse<object>()
                    {
                        Error = new Error<object>
                        {
                            Code = -32700,
                            Message = "Parse error",
                        }
                    });

                yield return (
                    new Request<object>()
                    {
                        Method = "params.http_code",
                        Params = new { statusCode = 500 }
                    },
                    new ErrorResponse<object>()
                    {
                        Error = new Error<object>
                        {
                            Code = -32603,
                            Message = "Internal error",
                        }
                    });

            }
        }
    }
}