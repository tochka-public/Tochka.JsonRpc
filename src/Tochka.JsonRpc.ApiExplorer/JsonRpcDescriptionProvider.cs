using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.ApiExplorer;

/// <inheritdoc />
/// <summary>
/// ApiDescriptionProvider that overrides default description for JSON-RPC API
/// </summary>
public class JsonRpcDescriptionProvider : IApiDescriptionProvider
{
    // need to run after DefaultApiDescriptionProvider to override it's result
    /// <inheritdoc />
    public int Order => -900;

    private readonly ITypeEmitter typeEmitter;
    private readonly ILogger<JsonRpcDescriptionProvider> log;

    /// <summary></summary>
    public JsonRpcDescriptionProvider(ITypeEmitter typeEmitter, ILogger<JsonRpcDescriptionProvider> log)
    {
        this.typeEmitter = typeEmitter;
        this.log = log;
    }

    /// <inheritdoc />
    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
        // default provider includes actions with [ApiController] and [Route]: isVisible && isAttributeRouted
        var existingDescriptions = context.Results
            .Where(static x => x.ActionDescriptor.EndpointMetadata.Any(static m => m is JsonRpcControllerAttribute))
            .ToList();

        foreach (var description in existingDescriptions)
        {
            // sanity checks
            if (description.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            {
                throw new InvalidOperationException($"Descriptor [{description.ActionDescriptor.DisplayName}] has invalid type");
            }

            var methodMetadata = actionDescriptor.EndpointMetadata.Get<JsonRpcMethodAttribute>();
            if (methodMetadata == null)
            {
                throw new InvalidOperationException($"Descriptor [{description.ActionDescriptor.DisplayName}] has no JsonRpcMethodAttribute");
            }

            description.HttpMethod = HttpMethods.Post;
            description.RelativePath += $"#{methodMetadata.Method}";
            description.Properties[ApiExplorerConstants.MethodNameProperty] = methodMetadata.Method;

            WrapRequest(description, actionDescriptor, methodMetadata.Method);
            WrapResponse(description, actionDescriptor, methodMetadata.Method);
        }
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
    }

    private void WrapRequest(ApiDescription description, ControllerActionDescriptor actionDescriptor, string methodName)
    {
        description.SupportedRequestFormats.Clear();
        foreach (var contentType in JsonRpcConstants.AllowedRequestContentType)
        {
            description.SupportedRequestFormats.Add(new ApiRequestFormat { MediaType = contentType });
        }

        var parametersMetadata = actionDescriptor.EndpointMetadata.Get<JsonRpcActionParametersMetadata>() ?? new JsonRpcActionParametersMetadata();
        var parametersToRemove = description.ParameterDescriptions
            .Where(p => parametersMetadata.Parameters.ContainsKey(p.Name) || p.Source == BindingSource.Body)
            .ToList();
        foreach (var parameterDescription in parametersToRemove)
        {
            description.ParameterDescriptions.Remove(parameterDescription);
        }

        var requestType = GetRequestType(actionDescriptor, parametersMetadata, methodName);
        description.ParameterDescriptions.Add(new ApiParameterDescription
        {
            Name = JsonRpcConstants.ParamsProperty,
            Source = BindingSource.Body,
            IsRequired = true,
            ModelMetadata = new JsonRpcModelMetadata(requestType),
            Type = requestType
        });
    }

    private Type GetRequestType(ControllerActionDescriptor actionDescriptor, JsonRpcActionParametersMetadata parametersMetadata, string methodName)
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

        return typeEmitter.CreateRequestType(GetActionFullName(actionDescriptor), methodName, baseParamsType, parametersBoundByDefault);
    }

    private void WrapResponse(ApiDescription description, ControllerActionDescriptor actionDescriptor, string methodName)
    {
        var resultType = description.SupportedResponseTypes.FirstOrDefault()?.Type ?? typeof(object);
        var responseType = typeEmitter.CreateResponseType(GetActionFullName(actionDescriptor), methodName, resultType);

        description.SupportedResponseTypes.Clear();

        description.SupportedResponseTypes.Add(new ApiResponseType
        {
            ApiResponseFormats = JsonRpcConstants.AllowedRequestContentType.Select(contentType => new ApiResponseFormat { MediaType = contentType }).ToArray(),
            IsDefaultResponse = false,
            StatusCode = 200,
            ModelMetadata = new JsonRpcModelMetadata(responseType),
            Type = responseType
        });
    }

    private static string GetActionFullName(ControllerActionDescriptor actionDescriptor) =>
        $"{actionDescriptor.ControllerTypeInfo.FullName}.{actionDescriptor.ActionName}";
}
