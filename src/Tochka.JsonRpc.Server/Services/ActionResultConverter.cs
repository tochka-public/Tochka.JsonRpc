using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services
{
    public class ActionResultConverter : IActionResultConverter
    {
        protected readonly JsonRpcFormatter JsonRpcFormatter;
        protected readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
        protected readonly ILogger log;
        protected readonly JsonRpcOptions options;

        public ActionResultConverter(JsonRpcFormatter jsonRpcFormatter, IJsonRpcErrorFactory jsonRpcErrorFactory, IOptions<JsonRpcOptions> options, ILogger<ActionResultConverter> log)
        {
            this.JsonRpcFormatter = jsonRpcFormatter;
            this.jsonRpcErrorFactory = jsonRpcErrorFactory;
            this.options = options.Value;
            this.log = log;
        }

        /// <summary>
        /// Create object result with proper formatter or handle special types to pass raw data
        /// </summary>
        /// <param name="actionResult"></param>
        /// <param name="metadata"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public virtual IActionResult ConvertActionResult(IActionResult actionResult, MethodMetadata metadata, IJsonRpcSerializer serializer)
        {
            switch (actionResult)
            {
                case ObjectResult result:
                    log.LogTrace($"Action returned object result [{result.GetType().Name}], value [{result.Value?.GetType().Name}], status code [{result.StatusCode}]");
                    // null code is normal so we pretend that it is good
                    // subclassed (probably bad) ObjectResults set code explicitly
                    return CreateObjectResult(result.StatusCode ?? 200, result.Value, serializer);

                case StatusCodeResult result:
                    // method returned only http code
                    log.LogTrace($"Action returned only http code result [{result.GetType().Name}], status code [{result.StatusCode}]");
                    return CreateObjectResult(result.StatusCode, null, serializer);

                case EmptyResult _:
                    // method was void (or returned null?)
                    log.LogTrace($"Action returned empty result");
                    return CreateObjectResult(200, null, serializer);

                default:
                    // return as is
                    if (options.AllowRawResponses)
                    {
                        log.LogTrace($"Action returned [{actionResult.GetType().Name}], return as raw");
                        return actionResult;
                    }
                    else
                    {
                        log.LogTrace($"Action returned [{actionResult.GetType().Name}], raw responses not allowed");
                        throw new JsonRpcInternalException($"Raw responses are not allowed by default, check {nameof(JsonRpcOptions)}", actionResult, metadata, null);
                    }
            }
        }

        public virtual IActionResult GetFailedBindingResult(ModelStateDictionary modelState)
        {
            var result = new BadRequestObjectResult(modelState);
            result.ContentTypes.Add(JsonRpcConstants.ContentType);
            return result;
        }

        protected internal virtual ObjectResult CreateObjectResult(int httpCode, object value, IJsonRpcSerializer serializer)
        {
            var response = MaybeHttpCodeErrorResponse(httpCode, value, serializer) ?? ResponseFromObject(value, serializer);
            var result = new ObjectResult(response)
            {
                StatusCode = 200,
            };
            result.Formatters.Add(JsonRpcFormatter);
            result.ContentTypes.Add(JsonRpcConstants.ContentType);
            return result;
        }

        /// <summary>
        /// Null for null and 2xx, ErrorResponse for others
        /// </summary>
        /// <param name="code"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        protected internal virtual IResponse MaybeHttpCodeErrorResponse(int code, object value, IJsonRpcSerializer serializer)
        {
            if (Utils.IsGoodHttpCode(code))
            {
                log.LogTrace($"HTTP Code [{code}] is good");
                return null;
            }

            log.LogTrace($"HTTP Code [{code}] is bad, return http error");
            var error = jsonRpcErrorFactory.HttpError(code, value);
            return new UntypedErrorResponse
            {
                Error = new Error<JToken>()
                {
                    Code = error.Code,
                    Message = error.Message,
                    Data = SerializeContent(error.GetData(), serializer)
                }
            };
        }

        /// <summary>
        /// Handle special types and wrap anything else
        /// </summary>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        protected internal virtual IResponse ResponseFromObject(object value, IJsonRpcSerializer serializer)
        {
            switch (value)
            {
                case IError error:
                    log.LogTrace($"Returned value is Error [{value.GetType().Name}], return as error");
                    return GetErrorResponse(error, serializer);
                    
                default:
                    log.LogTrace($"Returned value is normal object [{value?.GetType().Name}], return as response");
                    return new UntypedResponse()
                    {
                        Result = SerializeContent(value, serializer)
                    };
            }
        }

        protected internal virtual UntypedErrorResponse GetErrorResponse(IError error, IJsonRpcSerializer serializer)
        {
            return new UntypedErrorResponse
            {
                Error = new Error<JToken>()
                {
                    Code = error.Code,
                    Message = error.Message,
                    Data = SerializeContent(error.GetData(), serializer)
                }
            };
        }

        protected internal virtual JToken SerializeContent(object value, IJsonRpcSerializer serializer)
        {
            if (value == null)
            {
                return JValue.CreateNull();
            }
            return JToken.FromObject(value, serializer.Serializer);
        }
    }
}