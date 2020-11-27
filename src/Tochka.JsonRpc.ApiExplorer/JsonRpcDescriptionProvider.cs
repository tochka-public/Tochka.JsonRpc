using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.ApiExplorer
{
    /// <summary>
    /// Replaces ApiDescriptions after DefaultApiDescriptionProvider for JsonRpc actions
    /// </summary>
    public class JsonRpcDescriptionProvider : IApiDescriptionProvider
    {
        private readonly IMethodMatcher methodMatcher;
        private readonly ITypeEmitter typeEmitter;

        public JsonRpcDescriptionProvider(IMethodMatcher methodMatcher, ITypeEmitter typeEmitter)
        {
            this.methodMatcher = methodMatcher;
            this.typeEmitter = typeEmitter;
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            // will reuse some properties created by default provider
            var existingDescriptions = context.Results
                .Where(x => x.ActionDescriptor.GetProperty<MethodMetadata>() != null)
                .ToDictionary(x => x.ActionDescriptor);

            // clean up after default provider
            foreach (var apiDescription in existingDescriptions)
            {
                context.Results.Remove(apiDescription.Value);
            }

            var jsonRpcActions = context.Actions.Where(x => x.GetProperty<MethodMetadata>() != null);
            foreach (var action in jsonRpcActions)
            {
                var originalDescription = existingDescriptions[action];
                var methodMetadata = action.GetProperty<MethodMetadata>();
                var actionName = methodMatcher.GetActionName(methodMetadata);
                // TODO use options
                var defaultSerializerType = typeof(SnakeCaseJsonRpcSerializer);

                var apiDescription = new ApiDescription()
                {
                    ActionDescriptor = action,
                    HttpMethod = HttpMethods.Post,
                    RelativePath = GetUniquePath(originalDescription.RelativePath, actionName),
                    SupportedRequestFormats =
                    {
                        JsonRequestFormat
                    },
                    SupportedResponseTypes =
                    {
                        // Single because more than 1 response type is a complicated scenario, don't know how to deal with it
                        WrapResponseType(actionName, originalDescription.SupportedResponseTypes.SingleOrDefault()?.Type, methodMetadata)
                    },
                    GroupName = Utils.GetSwaggerFriendlyDocumentName(methodMetadata.MethodOptions.RequestSerializer, defaultSerializerType),
                    Properties =
                    {
                        [ApiExplorerConstants.ActionNameProperty] = actionName,
                    }
                };

                foreach (var parameterDescription in GetParameterDescriptions(actionName, originalDescription, methodMetadata))
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
        /// Add #method to JsonRpc urls to make them look different for swagger
        /// </summary>
        private string GetUniquePath(string path, string methodName) => $"{path}#{methodName}";

        /// <summary>
        /// Wrap action response type into generated Response`T, set content-type application/json and HTTP code 200
        /// </summary>
        private ApiResponseType WrapResponseType(string actionName, Type existingResponseType, MethodMetadata methodMetadata)
        {
            // If method returns void/Task/IActionResult, existingResponseType is null
            // If method returns object, SupportedResponseTypes is empty
            var methodReturnType = existingResponseType ?? typeof(object);
            var responseType = typeEmitter.CreateResponseType(actionName, methodReturnType, methodMetadata.MethodOptions.RequestSerializer);
            
            return new ApiResponseType()
            {
                ApiResponseFormats = JsonResponseFormat,
                IsDefaultResponse = false,
                ModelMetadata = new FakeMetadata(responseType),
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
                ModelMetadata = new FakeMetadata(requestType),
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
            else if (parameterBoundAsObject != null)
            {
                // use existing object as base to preserve property attributes
                baseType = parameterBoundAsObject.Type;
            }

            // compile type with properties corresponding to bound parameters
            // and add attribute with JsonRpcSerializer to be used later
            return typeEmitter.CreateRequestType(actionName, baseType, parametersBoundByDefault, methodMetadata.MethodOptions.RequestSerializer);
        }

        /// <summary>
        /// Arbitrary number greater than -1000 in DefaultApiDescriptionProvider
        /// </summary>
        public int Order => 100;

        /// <summary>
        /// Request content-type application/json
        /// </summary>
        private static ApiRequestFormat JsonRequestFormat =>
            new ApiRequestFormat()
            {
                MediaType = JsonRpcConstants.ContentType,
                Formatter = null
            };

        /// <summary>
        /// Response content-type application/json
        /// </summary>
        private static List<ApiResponseFormat> JsonResponseFormat => new List<ApiResponseFormat>()
        {
            new ApiResponseFormat()
            {
                MediaType = JsonRpcConstants.ContentType,
                Formatter = null
            }
        };
    }
}