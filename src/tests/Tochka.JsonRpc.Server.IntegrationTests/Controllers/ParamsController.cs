using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.IntegrationTests.Models;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.IntegrationTests.Controllers
{
    public class ParamsController : JsonRpcController
    {
        private readonly ILogger log;

        public ParamsController(ILogger<ParamsController> log)
        {
            this.log = log;
        }

        /// <summary>
        /// Tuple is serialized as {}, so we see it as different arguments
        /// </summary>
        public (int, string) Tuple(int item1, string item2)
        {
            return (item1, item2);
        }

        public StatusCodeResult HttpCode(int statusCode)
        {
            return StatusCode(statusCode);
        }

        public ObjectResult HttpObjectCode(int statusCode, object value)
        {
            return StatusCode(statusCode, value);
        }

        public TestData BindObject([FromParams(BindingStyle.Object)] TestData x)
        {
            return new TestData()
            {
                Value = x.Value + x.Value,
                OtherValue = x.OtherValue + x.OtherValue
            };
        }

        public int BindArray([FromParams(BindingStyle.Array)] List<int> x)
        {
            return x.Sum();
        }
    }
}