using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;
using Tochka.JsonRpc.V1.Server.IntegrationTests.Models;
using Tochka.JsonRpc.V1.Server.Pipeline;

namespace Tochka.JsonRpc.V1.Server.IntegrationTests.Controllers
{
    public class NoParamsController : JsonRpcController
    {
        private readonly ILogger log;

        public NoParamsController(ILogger<NoParamsController> log)
        {
            this.log = log;
        }

        public void Void()
        {
        }

        public object Null()
        {
            return null;
        }

        public string String()
        {
            return "test";
        }

        public List<int> List()
        {
            return new List<int>
            {
                1,
                2,
                3
            };
        }

        public dynamic Dynamic()
        {
            dynamic x = new ExpandoObject();
            x.test = 1;
            return x;
        }

        public TestData TestData()
        {
            return new TestData();
        }

        public ObjectResult ObjectResult()
        {
            return new ObjectResult(new TestData());
        }

        public OkResult OkResult()
        {
            return Ok();
        }

        public OkObjectResult OkObjectResult()
        {
            return Ok(new TestData());
        }

        public NoContentResult NoContentResult()
        {
            return NoContent();
        }

        public void Exception()
        {
            throw new DivideByZeroException("test");
        }

        public IError Error()
        {
            return  new Error<bool>
            {
                Code = 1,
                Message = "test",
                Data = true
            };
        }
    }
}
