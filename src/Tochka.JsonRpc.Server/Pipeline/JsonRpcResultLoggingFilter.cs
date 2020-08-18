using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Models;

namespace Tochka.JsonRpc.Server.Pipeline
{
    public class JsonRpcResultLoggingFilter : IActionFilter
    {
        private readonly ILogger log;

        public JsonRpcResultLoggingFilter(ILogger<JsonRpcResultLoggingFilter> log)
        {
            this.log = log;
        }


        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (!(context.Controller is JsonRpcController))
            {
                return;
            }

            log.LogTrace($"{nameof(OnActionExecuted)} Started");

            var methodMetadata = context.ActionDescriptor.GetProperty<MethodMetadata>() ?? throw new ArgumentNullException(nameof(MethodMetadata));
            var serializer = context.HttpContext.RequestServices.GetRequiredService(methodMetadata.MethodOptions.RequestSerializer) as IJsonRpcSerializer;
            switch (context.Result)
            {
                case ObjectResult objectResult:
                    var jsonValue = JsonConvert.SerializeObject(objectResult.Value, serializer.Settings);
                    log.LogInformation("JsonRpc Action ObjectResult {code}: {value}", objectResult.StatusCode, jsonValue);
                    break;
                default:
                    log.LogInformation("JsonRpc Action result [{type}]: {result}", context.Result.GetType(), context.Result);
                    break;
            }

            log.LogTrace($"{nameof(OnActionExecuting)} Completed");
        }
    }
}