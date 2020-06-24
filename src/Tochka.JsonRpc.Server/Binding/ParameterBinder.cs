using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Server.Models.Binding;

namespace Tochka.JsonRpc.Server.Binding
{
    public class ParameterBinder : IParameterBinder
    {
        private readonly ILogger log;

        public ParameterBinder(ILogger<ParameterBinder> log)
        {
            this.log = log;
        }

        public virtual Task SetResult(ModelBindingContext context, IParseResult result, string parameterName, RpcBindingContext rpcBindingContext)
        {
            log.LogTrace($"Binding parameter [{parameterName}]");
            switch (result)
            {
                case SuccessParseResult successBindResult:
                    return SetResultSafe(context, parameterName, successBindResult, rpcBindingContext.Serializer.Serializer);

                case ErrorParseResult errorBindResult:
                    return SetError(context, parameterName, errorBindResult);

                case NoParseResult noParseResult when rpcBindingContext.ParameterMetadata.IsOptional:
                    // key was not in json but is optional parameter
                    return SetNoResult(context, parameterName, noParseResult);

                case NoParseResult noBindResult:
                    // key was not in json and is required
                    return SetError(context, parameterName, noBindResult);

                case NullParseResult nullParseResult when context.ModelMetadata.IsReferenceOrNullableType:
                    // json value was null and type can be null
                    return SetNullResult(context, parameterName, nullParseResult);

                case NullParseResult nullParseResult:
                    // json value was null and type can not be null
                    var error = new ErrorParseResult($"Can not bind null json value to non-nullable parameter of type [{context.ModelMetadata.ModelType.Name}]", nullParseResult.Key);
                    return SetError(context, parameterName, error);

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result?.GetType().Name);
            }
        }

        protected internal virtual Task SetResultSafe(ModelBindingContext context, string parameterName, SuccessParseResult result, JsonSerializer serializer)
        {
            try
            {
                log.LogTrace($"{nameof(SetResultSafe)}, parameterName {parameterName}: [{result}]");
                SetDeserializedResult(context, result.Value, serializer);
            }
            catch (Exception e)
            {
                log.LogWarning(e, $"{nameof(SetResultSafe)}, parameterName {parameterName} failed result was [{result}]");
                context.ModelState.AddModelError(parameterName, $"{result}. {e.GetType().Name}: {e.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deserialization can fail
        /// </summary>
        /// <param name="context"></param>
        /// <param name="json"></param>
        /// <param name="serializer"></param>
        protected internal virtual void SetDeserializedResult(ModelBindingContext context, JToken json, JsonSerializer serializer)
        {
            log.LogTrace($"{nameof(SetDeserializedResult)} deserializing json to [{context.ModelMetadata.ModelType.Name}]");
            context.Result = ModelBindingResult.Success(json.ToObject(context.ModelMetadata.ModelType, serializer));
        }

        protected internal virtual Task SetError(ModelBindingContext context, string parameterName, IParseResult parseResult)
        {
            log.LogTrace($"{nameof(SetError)}, parameterName {parameterName}: [{parseResult}]");
            context.ModelState.AddModelError(parameterName, parseResult.ToString());
            return Task.CompletedTask;
        }

        protected internal virtual Task SetNullResult(ModelBindingContext context, string parameterName, NullParseResult nullParseResult)
        {
            log.LogTrace($"{nameof(SetNullResult)}, parameterName {parameterName}: [{nullParseResult}]");
            context.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        protected internal virtual Task SetNoResult(ModelBindingContext context, string parameterName, NoParseResult noParseResult)
        {
            log.LogTrace($"{nameof(SetNoResult)}, parameterName {parameterName}: [{noParseResult}]");
            return Task.CompletedTask;
        }
    }
}