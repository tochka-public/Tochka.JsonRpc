using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace WebApplication1.Controllers
{
    public class WtfProvider : IApiDescriptionProvider
    {
        private readonly IMethodMatcher methodMatcher;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;
        private readonly ModelTypeEmitter modelTypeEmitter;

        public WtfProvider(IMethodMatcher methodMatcher, IEnumerable<IJsonRpcSerializer> serializers)
        {
            this.methodMatcher = methodMatcher;
            this.serializers = serializers;
            modelTypeEmitter = new ModelTypeEmitter();
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            // clean up after DefaultApiDescriptionProvider
            var defaultDescriptions = context.Results
                .Where(x => x.ActionDescriptor.GetProperty<MethodMetadata>() != null)
                .ToDictionary(x => x.ActionDescriptor);
            foreach (var apiDescription in defaultDescriptions)
            {
                context.Results.Remove(apiDescription.Value);
            }

            var jsonRpcActions = context.Actions.Where(x => x.GetProperty<MethodMetadata>() != null);
            foreach (var action in jsonRpcActions)
            {
                var defaultDescription = defaultDescriptions[action];
                var methodMetadata = action.GetProperty<MethodMetadata>();
                var methodName = methodMatcher.GetActionName(methodMetadata);

                var apiDescription = new ApiDescription()
                {
                    ActionDescriptor = action,
                    HttpMethod = HttpMethods.Post,
                    RelativePath = GetUniquePath(defaultDescription.RelativePath, methodName),
                    SupportedRequestFormats = {JsonRequestFormat},
                    SupportedResponseTypes =
                    {
                        // more than 1 response type is a complicated scenario (dont know how to deal with it)
                        WrapResponseType(defaultDescription.SupportedResponseTypes.Single().Type)
                    }
                };

                foreach (var parameterDescription in GetParameterDescriptions(defaultDescription, methodMetadata))
                {
                    apiDescription.ParameterDescriptions.Add(parameterDescription);
                }

                context.Results.Add(apiDescription);
            }
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        /// <summary>
        /// Add #method to JsonRpc urls to make them different for swagger
        /// </summary>
        private string GetUniquePath(string path, string methodName) => $"{path}#{methodName}";

        /// <summary>
        /// Request content-type application/json
        /// </summary>
        private static ApiRequestFormat JsonRequestFormat =>
            new ApiRequestFormat()
            {
                Formatter = null,
                MediaType = JsonRpcConstants.ContentType
            };

        /// <summary>
        /// Wrap action response type into Response`T, set content-type application/json and HTTP code 200
        /// </summary>
        private static ApiResponseType WrapResponseType(Type existingResponseType)
        {
            // If method returns void/Task/IActionResult, existingResponseType is null
            var bodyType = existingResponseType ?? typeof(object);
            var responseType = typeof(Response<>).MakeGenericType(bodyType);
            
            return new ApiResponseType()
            {
                ApiResponseFormats = new List<ApiResponseFormat>()
                {
                    new ApiResponseFormat()
                    {
                        MediaType = JsonRpcConstants.ContentType,
                        Formatter = null
                    }
                },
                IsDefaultResponse = false,
                ModelMetadata = new DumbMetadata(ModelMetadataIdentity.ForType(responseType)),
                StatusCode = (int)HttpStatusCode.OK,
                Type = responseType
            };
        }

        /// <summary>
        /// Wrap JsonRpc-bound parameters into Request`T. T is compiled dynamically to allow swagger schema generation
        /// </summary>
        /// <param name="defaultDescription"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private IEnumerable<ApiParameterDescription> GetParameterDescriptions(ApiDescription defaultDescription, MethodMetadata methodMetadata)
        {
            var requestType = GetRequestType(methodMetadata);
            var jsonRpcParamsDescription = new ApiParameterDescription()
            {
                Name = "params",
                Source = BindingSource.Body,
                ParameterDescriptor = null,
                DefaultValue = null,
                IsRequired = true,
                ModelMetadata = new DumbMetadata(ModelMetadataIdentity.ForType(requestType)), // swagger generator uses ModelMetadata.ModelType
                RouteInfo = null,
                Type = requestType
            };
            
            var otherParameters = defaultDescription.ParameterDescriptions
                .Where(x => x.ParameterDescriptor.BindingInfo.BinderType != typeof(JsonRpcModelBinder));

            return new List<ApiParameterDescription>
                {
                    jsonRpcParamsDescription
                }
                .Concat(otherParameters);
        }

        /// <summary>
        /// Generate type which describes whole request object with all parameters bound by json rpc
        /// </summary>
        /// <param name="defaultDescription"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private Type GetRequestType(MethodMetadata methodMetadata)
        {
            // TODO we have all info about types and serialization here.
            // TODO how to pass this downstream for proper JSON serialization?
            // emit attributes?
            // use dictionary instead of type?
            // hack downstream schema generator? (still need to pass info?)
            var serializer = serializers.First(x => x.GetType() == methodMetadata.MethodOptions.RequestSerializer);
            // TODO свой ISerializerDataContractResolver который выбирает json settings и json contract resolver?





            var parameterBoundAsObject = methodMetadata.Parameters.Values.FirstOrDefault(x => x.BindingStyle == BindingStyle.Object);
            var parameterBoundAsArray = methodMetadata.Parameters.Values.FirstOrDefault(x => x.BindingStyle == BindingStyle.Array);
            var parametersBoundByDefault = methodMetadata.Parameters.Values
                .Where(x => x.BindingStyle == BindingStyle.Default)
                .ToDictionary(x => x.Name.Original, x => x.Type);

            if (parameterBoundAsArray != null)
            {
                // return Request<List<T>> (or whatever collection is used)
                // because other stuff won't bind if its type is different from T
                // so no difference for user, it is always visible as List<T>
                return typeof(Request<>).MakeGenericType(parameterBoundAsArray.Type);
            }

            if (parameterBoundAsObject != null && !parametersBoundByDefault.Any())
            {
                // only whole object is bound, return Request<T>
                return typeof(Request<>).MakeGenericType(parameterBoundAsObject.Type);
            }

            if (parameterBoundAsObject != null)
            {
                // combine default-bound parameters with bound object properties if present
                foreach (var propertyInfo in parameterBoundAsObject.Type.GetProperties())
                {
                    parametersBoundByDefault[propertyInfo.Name] = propertyInfo.PropertyType;
                }
            }

            // compile type with properties corresponding to bound parameters
            var requestBodyType = modelTypeEmitter.CreateModelType(methodMetadata.Action.Original, parametersBoundByDefault);
            return typeof(Request<>).MakeGenericType(requestBodyType);
        }

        /// <summary>
        /// Arbitrary number greater than -1000 in DefaultApiDescriptionProvider
        /// </summary>
        public int Order => 100;

        /*
        private void PatchParams(ApiDescription apiDescription, MethodMetadata methodMetadata, string methodName)
        {
            var requestType = GetRequestType(apiDescription, methodMetadata);

            var allJsonRpcParams = apiDescription.ParameterDescriptions
                .Where(x => x.ParameterDescriptor.BindingInfo.BinderType == typeof(JsonRpcModelBinder))
                .ToList();

            // hide params bound by JsonRpc
            foreach (var apiParameterDescription in allJsonRpcParams)
            {
                apiDescription.ParameterDescriptions.Remove(apiParameterDescription);
            }

            var jsonRpcParamsDescription = new ApiParameterDescription()
            {
                Name = "params",
                Source = BindingSource.Body,
                ParameterDescriptor = null,
                DefaultValue = null,
                IsRequired = true,
                ModelMetadata = new DumbMetadata(ModelMetadataIdentity.ForType(requestType)), // swagger generator uses ModelMetadata.ModelType
                RouteInfo = null,
                Type = requestType
            };
            apiDescription.ParameterDescriptions.Add(jsonRpcParamsDescription);

            // TODO use schema filters?
            return;
            schemaGeneratorOptions.CustomTypeMappings[requestType] = () => new OpenApiSchema()
            {
                Example = new OpenApiObject()
                {
                    {"id", new OpenApiString("the_id")},
                    {"jsonrpc", new OpenApiString("1112.0")},
                    {"method", new OpenApiString(methodName)},
                    {"params", new OpenApiObject()},
                    // uhh... what next? maybe use schema filters?
                }
            };



            // TODO: apply custom serialization for examples
            // TODO: XMLDOC jsonRpc = 2.0 works, but will it work for referenced nuget library?
            // TODO: XMLDOC id does not work because it is object?
        }
        */

    }
}