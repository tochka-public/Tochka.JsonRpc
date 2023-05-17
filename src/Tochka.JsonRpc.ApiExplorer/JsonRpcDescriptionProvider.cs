using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.ApiExplorer;

public class JsonRpcDescriptionProvider : IApiDescriptionProvider
{
    // need to run after DefaultApiDescriptionProvider to override it's result
    public int Order => int.MaxValue;

    private readonly ITypeEmitter typeEmitter;

    public JsonRpcDescriptionProvider(ITypeEmitter typeEmitter) => this.typeEmitter = typeEmitter;

    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
        var existingDescriptions = context.Results
            .Where(static x => x.ActionDescriptor.EndpointMetadata.Any(static m => m is JsonRpcControllerAttribute));

        foreach (var description in existingDescriptions)
        {
            var methodMetadata = description.ActionDescriptor.EndpointMetadata.Get<JsonRpcMethodAttribute>();
            if (methodMetadata == null)
            {
                // Should not be possible, sanity check
                context.Results.Remove(description);
                continue;
            }

            if (description.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            {
                // Should not be possible, sanity check
                continue;
            }

            var serializerMetadata = actionDescriptor.EndpointMetadata.Get<JsonRpcSerializerOptionsAttribute>();
            var serializerOptionsProviderType = serializerMetadata?.ProviderType;

            description.GroupName = Utils.GetDocumentName(serializerOptionsProviderType);

            description.HttpMethod = HttpMethods.Post;
            description.RelativePath += $"#{methodMetadata.Method}";

            WrapRequest(description, actionDescriptor, methodMetadata.Method, serializerOptionsProviderType);
            WrapResponse(description, methodMetadata.Method, serializerOptionsProviderType);
        }
    }

    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
    }

    private void WrapRequest(ApiDescription description, ActionDescriptor actionDescriptor, string methodName, Type? serializerOptionsProviderType)
    {
        description.SupportedRequestFormats.Clear();
        description.SupportedRequestFormats.Add(new ApiRequestFormat { MediaType = JsonRpcConstants.ContentType });

        var parametersMetadata = actionDescriptor.EndpointMetadata.Get<JsonRpcActionParametersMetadata>();
        if (parametersMetadata == null)
        {
            // Should not be possible, sanity check
            return;
        }

        var jsonRpcParameters = description.ParameterDescriptions
            .Where(p => parametersMetadata.Parameters.ContainsKey(p.Name) || p.Source == BindingSource.Body)
            .ToArray();
        foreach (var parameterDescription in jsonRpcParameters)
        {
            description.ParameterDescriptions.Remove(parameterDescription);
        }

        var requestType = GetRequestType(parametersMetadata, methodName, serializerOptionsProviderType);
        description.ParameterDescriptions.Add(new ApiParameterDescription
        {
            Name = "params",
            Source = BindingSource.Body,
            IsRequired = true,
            ModelMetadata = new JsonRpcModelMetadata(requestType),
            Type = requestType
        });
    }

    private Type GetRequestType(JsonRpcActionParametersMetadata parametersMetadata, string methodName, Type? serializerOptionsProviderType)
    {
        var parameters = parametersMetadata.Parameters.Values;
        var parameterBoundAsObject = parameters.FirstOrDefault(static x => x.BindingStyle == BindingStyle.Object);
        var parameterBoundAsArray = parameters.FirstOrDefault(static x => x.BindingStyle == BindingStyle.Array);
        var parametersBoundByDefault = parameters
            .Where(static x => x.BindingStyle == BindingStyle.Default)
            .ToDictionary(static x => x.OriginalName, static x => x.Type);

        var baseParamsType = typeof(object);
        if (parameterBoundAsArray != null)
        {
            // inherit List<T> (or whatever collection is used)
            // and ignore other parameters
            // because other stuff won't bind if its type is different from T
            // so no difference for user, it is always visible as List<T>
            baseParamsType = parameterBoundAsArray.Type;
            parametersBoundByDefault.Clear();
        }
        else if (parameterBoundAsObject != null)
        {
            // use existing object as base to preserve property attributes
            baseParamsType = parameterBoundAsObject.Type;
        }

        return typeEmitter.CreateRequestType(methodName, baseParamsType, parametersBoundByDefault, serializerOptionsProviderType);
    }

    private void WrapResponse(ApiDescription description, string methodName, Type? serializerOptionsProviderType)
    {
        var resultType = description.SupportedResponseTypes.FirstOrDefault()?.Type ?? typeof(object);
        var responseType = typeEmitter.CreateResponseType(methodName, resultType, serializerOptionsProviderType);

        description.SupportedResponseTypes.Clear();
        description.SupportedResponseTypes.Add(new ApiResponseType
        {
            ApiResponseFormats = { new ApiResponseFormat { MediaType = JsonRpcConstants.ContentType } },
            IsDefaultResponse = false,
            StatusCode = 200,
            ModelMetadata = new JsonRpcModelMetadata(responseType),
            Type = responseType
        });
    }
}
