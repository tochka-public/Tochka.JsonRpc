using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace WebApplication1.Services
{
    public class WtfProvider : IApiDescriptionProvider
    {
        private readonly IMethodMatcher methodMatcher;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;
        private readonly ITypeEmitter typeEmitter;

        public WtfProvider(IMethodMatcher methodMatcher, IEnumerable<IJsonRpcSerializer> serializers, ITypeEmitter typeEmitter)
        {
            this.methodMatcher = methodMatcher;
            this.serializers = serializers;
            this.typeEmitter = typeEmitter;
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
                var actionName = methodMatcher.GetActionName(methodMetadata);

                var apiDescription = new ApiDescription()
                {
                    ActionDescriptor = action,
                    HttpMethod = HttpMethods.Post,
                    RelativePath = GetUniquePath(defaultDescription.RelativePath, actionName),
                    SupportedRequestFormats = {JsonRequestFormat},
                    SupportedResponseTypes =
                    {
                        // more than 1 response type is a complicated scenario (dont know how to deal with it)
                        WrapResponseType(actionName, defaultDescription.SupportedResponseTypes.Single().Type, methodMetadata)
                    }
                };

                foreach (var parameterDescription in GetParameterDescriptions(actionName, defaultDescription, methodMetadata))
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
        private ApiResponseType WrapResponseType(string actionName, Type existingResponseType, MethodMetadata methodMetadata)
        {
            // If method returns void/Task/IActionResult, existingResponseType is null
            var methodReturnType = existingResponseType ?? typeof(object);
            var responseType = typeEmitter.CreateModelType(actionName, typeof(Response<>), methodReturnType, new Dictionary<string, Type>(), methodMetadata.MethodOptions.RequestSerializer);
            
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
        /// <param name="actionName"></param>
        /// <param name="defaultDescription"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private IEnumerable<ApiParameterDescription> GetParameterDescriptions(string actionName, ApiDescription defaultDescription, MethodMetadata methodMetadata)
        {
            var requestType = GetRequestType(actionName, methodMetadata);
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
        /// <param name="actionName"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private Type GetRequestType(string actionName, MethodMetadata methodMetadata)
        {
            // TODO we have all info about types and serialization here.
            // TODO how to pass this downstream for proper JSON serialization?
            // emit attributes?
            // use dictionary instead of type?
            // hack downstream schema generator? (still need to pass info?)
            // TODO свой ISerializerDataContractResolver который выбирает json settings и json contract resolver? или уровнем выше?
            




            var parameterBoundAsObject = methodMetadata.Parameters.Values.FirstOrDefault(x => x.BindingStyle == BindingStyle.Object);
            var parameterBoundAsArray = methodMetadata.Parameters.Values.FirstOrDefault(x => x.BindingStyle == BindingStyle.Array);
            var parametersBoundByDefault = methodMetadata.Parameters.Values
                .Where(x => x.BindingStyle == BindingStyle.Default)
                .ToDictionary(x => x.Name.Original, x => x.Type);

            var baseType = typeof(object);
            
            if (parameterBoundAsArray != null)
            {
                // inherit List<T> (or whatever collection is used)
                // and ignore other parameters
                // because other stuff won't bind if its type is different from T
                // so no difference for user, it is always visible as List<T>
                baseType = parameterBoundAsArray.Type;
                parametersBoundByDefault.Clear();
            }

            if (parameterBoundAsObject != null)
            {
                // use existing object as base to preserve property attributes
                baseType = parameterBoundAsObject.Type;
            }

            // compile type with properties corresponding to bound parameters
            // and add attribute with JsonRpcSerializer to be used later
            return typeEmitter.CreateModelType(actionName, typeof(Request<>), baseType, parametersBoundByDefault, methodMetadata.MethodOptions.RequestSerializer);
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

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenJsonRpcSupport(
            this IServiceCollection services)
        {
            // we still need original contract resolver
            services.AddTransient(s => new NewtonsoftDataContractResolver(s.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value, s.GetJsonSerializerSettings() ?? new JsonSerializerSettings()));
            // override with our service
            return services.Replace(ServiceDescriptor.Transient<ISerializerDataContractResolver, JsonRpcSerializerDataContractResolverNotUsed>());
        }

        /// <summary>
        /// Copy-pasted from swashbuckle library
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static JsonSerializerSettings GetJsonSerializerSettings(
            this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value?.SerializerSettings;
        }
    }
}