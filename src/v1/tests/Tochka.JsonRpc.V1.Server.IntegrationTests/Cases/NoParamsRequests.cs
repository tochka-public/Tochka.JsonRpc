using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Models.Response;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;
using Tochka.JsonRpc.V1.Server.IntegrationTests.Models;

namespace Tochka.JsonRpc.V1.Server.IntegrationTests.Cases
{
    /// <summary>
    /// Test default method matching. "Server MUST reply with a Response"
    /// </summary>
    public class NoParamsRequests
    {
        public static IEnumerable Cases => Requests.Select(x => new TestCaseData(x.data, x.expected));

        private static IEnumerable<(object data, object expected)> Requests
        {
            get
            {
                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.void"
                    },
                    new Response<object>()
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.null"
                    },
                    new Response<object>()
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.string"
                    },
                    new Response<string>()
                    {
                        Result = "test"
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.list"
                    },
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
                    new UntypedRequest()
                    {
                        Method = "no_params.dynamic"
                    },
                    new Response<Dictionary<string, object>>()
                    {
                        Result = new Dictionary<string, object>()
                        {
                            { "test", 1}
                        }
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.test_data"
                    },
                    new Response<TestData>()
                    {
                        Result = new TestData()
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.object_result"
                    },
                    new Response<TestData>()
                    {
                        Result = new TestData()
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.ok_result"
                    },
                    new Response<object>()
                    {
                        Result = null
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.ok_object_result"
                    },
                    new Response<TestData>()
                    {
                        Result = new TestData()
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.no_content_result"
                    },
                    new Response<object>()
                    {
                        Result = null
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.exception"
                    },
                    new ErrorResponse<ExceptionInfo>()
                    {
                        Error = new Error<ExceptionInfo>
                        {
                            Code = -32000,
                            Message = "Server error",
                            Data = new ExceptionInfo()
                            {
                                Message = "test",
                                Type = "System.DivideByZeroException"
                            }
                        }
                    }
                );

                yield return (
                    new UntypedRequest()
                    {
                        Method = "no_params.error"
                    },
                    new ErrorResponse<bool>()
                    {
                        Error = new Error<bool>
                        {
                            Code = 1,
                            Message = "test",
                            Data = true
                        }
                    }
                );
            }
        }
    }
}
