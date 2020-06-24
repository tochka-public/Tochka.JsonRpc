using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Server.IntegrationTests.Cases
{
    /// <summary>
    /// Test default parameter binding
    /// </summary>
    public class ParamsRequests
    {
        public static IEnumerable Cases => Requests.Select(x => new TestCaseData(x.data, x.expected));

        private static IEnumerable<(object data, object expected)> Requests
        {
            get
            {
                yield return (
                    new Request<(int, string)>()
                    {
                        Method = "params.tuple",
                        Params = (1, "test")
                    },
                    new Response<(int, string)>()
                    {
                        Result = (1, "test")
                    }
                );

            }
        }
    }
}