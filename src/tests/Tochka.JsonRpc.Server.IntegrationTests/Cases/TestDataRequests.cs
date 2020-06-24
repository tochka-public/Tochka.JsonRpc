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
    /// Test BindingStyle parameters binding
    /// </summary>
    public class TestDataRequests
    {
        public static IEnumerable Cases => Requests.Select(x => new TestCaseData(x.data, x.expected));

        private static IEnumerable<(object data, object expected)> Requests
        {
            get
            {
                yield return (
                    new Request<TestData>()
                    {
                        Method = "params.bind_object",
                        Params = new TestData()
                    },
                    new Response<TestData>()
                    {
                        Result = new TestData
                        {
                            Value = "testtest",
                            OtherValue = 2
                        }
                    });

                yield return (
                    new Request<List<int>>()
                    {
                        Method = "params.bind_array",
                        Params = new List<int> {1, 2, 3}
                    },
                    new Response<int>()
                    {
                        Result = 6
                    });

                yield return (
                    new Request<List<int>>()
                    {
                        Method = "params.bind_object",
                        Params = new List<int> { 1, 2, 3 }
                    },
                    new ErrorResponse<Dictionary<string, List<string>>>()
                    {
                        Error = new Error<Dictionary<string, List<string>>>
                        {
                            Code = -32602,
                            Message = "Invalid params",
                            Data = new Dictionary<string, List<string>>()
                            {
                                {"x", new List<string>(){ "Bind error. Can not bind array to object parameter. Json key [0]" } }
                            }
                        }
                    });

                yield return (
                    new Request<TestData>()
                    {
                        Method = "params.bind_array",
                        Params = new TestData()
                    },
                    new ErrorResponse<Dictionary<string,List<string>>>()
                    {
                        Error = new Error<Dictionary<string, List<string>>>
                        {
                            Code = -32602,
                            Message = "Invalid params",
                            Data = new Dictionary<string, List<string>>()
                            {
                                {"x", new List<string>(){ "Bind error. Can not bind object to collection parameter. Json key [x]" } }
                            }
                        }
                    });
            }
        }
    }
}