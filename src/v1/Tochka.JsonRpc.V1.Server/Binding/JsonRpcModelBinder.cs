using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server.Conventions;
using Tochka.JsonRpc.V1.Server.Models;
using Tochka.JsonRpc.V1.Server.Models.Binding;

namespace Tochka.JsonRpc.V1.Server.Binding
{
    public class JsonRpcModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext context)
        {
            var parameterName = context.ModelMetadata.Name;
            var rpcBindingContext = GetRpcBindingContext(context, parameterName);
            var parser = context.HttpContext.RequestServices.GetRequiredService<IParamsParser>();
            var result = parser.ParseParams(rpcBindingContext.Call.Params, rpcBindingContext.ParameterMetadata);
            var parameterBinder = context.HttpContext.RequestServices.GetRequiredService<IParameterBinder>();
            return parameterBinder.SetResult(context, result, parameterName, rpcBindingContext);
        }

        internal JsonRpcBindingContext GetRpcBindingContext(ModelBindingContext context, string parameterName)
        {
            var call = context.HttpContext.GetJsonRpcCall();
            
            var methodMetadata = context.ActionContext.ActionDescriptor.GetProperty<MethodMetadata>();
            if (methodMetadata == null)
            {
                throw new ArgumentNullException(nameof(methodMetadata), $"{nameof(MethodMetadata)} should be populated by {nameof(ActionConvention)} on application start");
            }

            var parameterMetadata = methodMetadata.Get(parameterName);
            if (parameterMetadata == null)
            {
                throw new ArgumentNullException(nameof(parameterMetadata), $"Can not find parameter metadata [{parameterName}]");
            }

            // DI requires to register binder provider which is global and we dont want it, so resolve manually
            var serializers = context.HttpContext.RequestServices.GetServices<IJsonRpcSerializer>();
            var serializer = Utils.GetSerializer(serializers, methodMetadata.MethodOptions.RequestSerializer);

            return new JsonRpcBindingContext
            {
                Call = call,
                ParameterMetadata = parameterMetadata,
                Serializer = serializer
            };
        }
    }
}
