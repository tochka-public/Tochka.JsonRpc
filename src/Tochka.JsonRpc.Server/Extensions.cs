using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Conventions;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server
{
    public static class Extensions
    {
        public static IMvcBuilder AddJsonRpcServer(this IMvcBuilder mvcBuilder, Action<JsonRpcOptions> configureOptions=null)
        {
            var services = mvcBuilder.Services;
            if(services == null) throw new ArgumentNullException();
            services.Configure(configureOptions ?? (options => { }));

            // mvc integration
            services.TryAddConvention<ControllerConvention>();
            services.TryAddConvention<ActionConvention>();
            services.TryAddConvention<ParameterConvention>();
            services.AddSingleton<IStartupFilter, JsonRpcStartupFilter>();
            services.TryAddSingleton<JsonRpcFormatter>();
            services.TryAddSingleton<JsonRpcModelBinder>();
            services.TryAddScoped<JsonRpcFilter>();

            // required user-overridable services
            services.TryAddSingleton<IParameterBinder, ParameterBinder>();
            services.TryAddSingleton<IRequestHandler, RequestHandler>();
            services.TryAddSingleton<IRequestReader, RequestReader>();
            services.TryAddSingleton<IResponseReader, ResponseReader>();
            services.TryAddSingleton<IJsonRpcErrorFactory, JsonRpcErrorFactory>();
            services.TryAddSingleton<IMethodMatcher, MethodMatcher>();
            services.TryAddSingleton<IJsonRpcRoutes, JsonRpcRoutes>();
            services.TryAddSingleton<IParamsParser, ParamsParser>();
            services.TryAddScoped<IActionResultConverter, ActionResultConverter>();
            services.TryAddTransient<INestedContextFactory, NestedContextFactory>();
            
            // required non-overridable services
            services.TryAddJsonRpcSerializer<HeaderJsonRpcSerializer>();
            services.TryAddJsonRpcSerializer<SnakeCaseJsonRpcSerializer>();

            return mvcBuilder;
        }

        public static IServiceCollection TryAddJsonRpcSerializer<T>(this IServiceCollection services)
        where T: class, IJsonRpcSerializer
        {
            // this is wrong because resolves two different instances, but we don't care, they are true singletons under the hood
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IJsonRpcSerializer), typeof(T), ServiceLifetime.Singleton));
            services.TryAddSingleton<T>();
            return services;
        }

        public static IUntypedCall GetJsonRpcCall(this HttpContext context) => (IUntypedCall)context.Items[JsonRpcConstants.RequestItemKey];

        /// <summary>
        /// Get serialized property/method/action/controller/parameter JSON name
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static JsonName GetJsonName(this IJsonRpcSerializer serializer, string name)
        {
            if (!(serializer.Serializer.ContractResolver is DefaultContractResolver resolver))
            {
                throw new ArgumentException($"Only {nameof(DefaultContractResolver)} or its descendants are supported");
            }

            var jsonName = resolver.GetResolvedPropertyName(name);
            return new JsonName(name, jsonName);
        }

        /// <summary>
        /// Serialize Exception as json response
        /// </summary>
        /// <param name="errorFactory"></param>
        /// <param name="e"></param>
        /// <param name="headerJsonRpcSerializer"></param>
        /// <returns></returns>
        public static JToken ConvertExceptionToResponse(this IJsonRpcErrorFactory errorFactory, Exception e, HeaderJsonRpcSerializer headerJsonRpcSerializer)
        {
            return errorFactory.ConvertErrorToResponse(errorFactory.Exception(e), headerJsonRpcSerializer);
        }

        /// <summary>
        /// Serialize Error as json response
        /// </summary>
        /// <param name="errorFactory"></param>
        /// <param name="value"></param>
        /// <param name="headerJsonRpcSerializer"></param>
        /// <returns></returns>
        public static JToken ConvertErrorToResponse(this IJsonRpcErrorFactory errorFactory, IError value, HeaderJsonRpcSerializer headerJsonRpcSerializer)
        {
            var error = new ErrorResponse<object>
            {
                Error = new Error<object>
                {
                    Code = value.Code,
                    Message = value.Message, 
                    Data = value.GetData()
                }
            };
            return JToken.FromObject(error, headerJsonRpcSerializer.Serializer);
        }

        internal static IServiceCollection TryAddConvention<T>(this IServiceCollection serviceCollection)
            where T : class
        {
            serviceCollection.TryAddSingleton<T>();
            serviceCollection.TryAddEnumerable(new ServiceDescriptor(typeof(IConfigureOptions<MvcOptions>), typeof(ConventionConfigurator<T>), ServiceLifetime.Singleton));
            return serviceCollection;
        }
    }
}