using System.Collections;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;
using Utils = Tochka.JsonRpc.Server.Utils;

namespace Tochka.JsonRpc.OpenRpc;

public class OpenRpcDocumentGenerator : IOpenRpcDocumentGenerator
{
    private readonly IApiDescriptionGroupCollectionProvider apiDescriptionsProvider;
    private readonly IOpenRpcSchemaGenerator schemaGenerator;
    private readonly IOpenRpcContentDescriptorGenerator contentDescriptorGenerator;
    private readonly IEnumerable<IJsonSerializerOptionsProvider> jsonSerializerOptionsProviders;
    private readonly JsonRpcServerOptions serverOptions;
    private readonly OpenRpcOptions openRpcOptions;

    public OpenRpcDocumentGenerator(IApiDescriptionGroupCollectionProvider apiDescriptionsProvider, IOpenRpcSchemaGenerator schemaGenerator, IOpenRpcContentDescriptorGenerator contentDescriptorGenerator, IEnumerable<IJsonSerializerOptionsProvider> jsonSerializerOptionsProviders, IOptions<JsonRpcServerOptions> serverOptions, IOptions<OpenRpcOptions> openRpcOptions)
    {
        this.apiDescriptionsProvider = apiDescriptionsProvider;
        this.schemaGenerator = schemaGenerator;
        this.contentDescriptorGenerator = contentDescriptorGenerator;
        this.jsonSerializerOptionsProviders = jsonSerializerOptionsProviders;
        this.serverOptions = serverOptions.Value;
        this.openRpcOptions = openRpcOptions.Value;
    }

    public Models.OpenRpc Generate(OpenRpcInfo info, string documentName, Uri host) =>
        new(info)
        {
            Servers = GetServers(host, serverOptions.RoutePrefix),
            Methods = GetMethods(documentName, host),
            Components = new()
            {
                Schemas = schemaGenerator.GetAllSchemas()
            }
        };

    private List<OpenRpcServer> GetServers(Uri host, string route)
    {
        var uriBuilder = new UriBuilder(host) { Path = route };
        var server = new OpenRpcServer(openRpcOptions.DefaultServerName, uriBuilder.Uri);

        return new List<OpenRpcServer>
        {
            server
        };
    }

    private List<OpenRpcMethod> GetMethods(string documentName, Uri host) =>
        apiDescriptionsProvider.ApiDescriptionGroups.Items
            .SelectMany(static g => g.Items)
            .Where(d => !openRpcOptions.IgnoreObsoleteActions || d.IsObsoleteTransitive())
            .Where(static d => d.ActionDescriptor.EndpointMetadata.Any(static m => m is JsonRpcControllerAttribute))
            .Where(d => openRpcOptions.DocInclusionPredicate(documentName, d))
            .Select(d => GetMethod(d, host))
            .OrderBy(static m => m.Name)
            .ToList();

    private OpenRpcMethod GetMethod(ApiDescription apiDescription, Uri host)
    {
        var methodInfo = (apiDescription.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
        var parametersMetadata = apiDescription.ActionDescriptor.EndpointMetadata.Get<JsonRpcActionParametersMetadata>();
        var serializerMetadata = apiDescription.ActionDescriptor.EndpointMetadata.Get<JsonRpcSerializerOptionsAttribute>();
        var jsonSerializerOptionsProviderType = serializerMetadata?.ProviderType;
        var jsonSerializerOptions = jsonSerializerOptionsProviderType == null
            ? serverOptions.DefaultDataJsonSerializerOptions
            : Utils.GetJsonSerializerOptions(jsonSerializerOptionsProviders, jsonSerializerOptionsProviderType);
        var methodName = (string) apiDescription.Properties[ApiExplorerConstants.MethodNameProperty];
        return new OpenRpcMethod(methodName)
        {
            Summary = methodInfo?.GetXmlDocsSummary(),
            Description = methodInfo?.GetXmlDocsRemarks(),
            Params = GetMethodParams(apiDescription, methodName, parametersMetadata, jsonSerializerOptions).ToList(),
            Result = GetResultContentDescriptor(apiDescription, methodName, jsonSerializerOptions),
            Deprecated = apiDescription.IsObsoleteTransitive(),
            Servers = GetMethodServers(apiDescription, host),
            ParamStructure = GetParamsStructure(parametersMetadata)
        };
    }

    private IEnumerable<OpenRpcContentDescriptor> GetMethodParams(ApiDescription apiDescription, string methodName, JsonRpcActionParametersMetadata? parametersMetadata, JsonSerializerOptions jsonSerializerOptions)
    {
        // there must be only one body parameter with Request<T> type
        var requestType = apiDescription.ParameterDescriptions.Single(static x => x.Source == BindingSource.Body).Type;
        // unpack Request<T>
        var bodyType = requestType.BaseType!.GenericTypeArguments.Single();

        var isCollection = typeof(IEnumerable).IsAssignableFrom(bodyType);
        var itemType = bodyType.GetEnumerableItemType();
        if (isCollection && itemType != null)
        {
            yield return contentDescriptorGenerator.GenerateForType(itemType.ToContextualType(), methodName, jsonSerializerOptions);

            yield break;
        }

        foreach (var propertyInfo in bodyType.GetProperties())
        {
            var property = propertyInfo.ToContextualProperty();
            if (parametersMetadata?.Parameters.TryGetValue(propertyInfo.Name, out var parameterMetadata) == true)
            {
                yield return contentDescriptorGenerator.GenerateForParameter(property.PropertyType, methodName, parameterMetadata, jsonSerializerOptions);
            }
            else
            {
                yield return contentDescriptorGenerator.GenerateForProperty(property, methodName, jsonSerializerOptions);
            }
        }
    }

    private OpenRpcContentDescriptor GetResultContentDescriptor(ApiDescription apiDescription, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        // there must be only one response type with Response<T> type
        var responseType = apiDescription.SupportedResponseTypes.Single().Type!;
        // unpack Response<T>
        var bodyType = responseType.BaseType!.GenericTypeArguments.Single().ToContextualType();
        return contentDescriptorGenerator.GenerateForType(bodyType, methodName, jsonSerializerOptions);
    }

    private List<OpenRpcServer>? GetMethodServers(ApiDescription apiDescription, Uri host)
    {
        var route = apiDescription.RelativePath?.Split('#').First();

        if (string.IsNullOrWhiteSpace(route) || $"/{route}" == serverOptions.RoutePrefix)
        {
            return null;
        }

        return GetServers(host, route);
    }

    private static OpenRpcParamStructure GetParamsStructure(JsonRpcActionParametersMetadata? parametersMetadata)
    {
        var bindingStyles = parametersMetadata?.Parameters.Values
                .Select(static p => p.BindingStyle)
                .Distinct()
                .ToHashSet()
            ?? new HashSet<BindingStyle>();

        return bindingStyles.Count switch
        {
            0 => OpenRpcParamStructure.Either,
            1 => bindingStyles.Single().ToParamStructure(),
            _ => CombineBindingStyles(bindingStyles)
        };
    }

    // mixed binding is bad but it's up to user to try this out
    private static OpenRpcParamStructure CombineBindingStyles(IReadOnlySet<BindingStyle> bindingStyles)
    {
        var hasArrayBinding = bindingStyles.Contains(BindingStyle.Array);
        var hasObjectBinding = bindingStyles.Contains(BindingStyle.Object);
        if (hasArrayBinding && !hasObjectBinding)
        {
            // if 'array' is present, 'default' binding works, 'object' binding does not
            return OpenRpcParamStructure.ByPosition;
        }

        if (hasObjectBinding && !hasArrayBinding)
        {
            // if 'object' is present, 'default' binding works, 'array' binding does not
            return OpenRpcParamStructure.ByName;
        }

        // both 'object' and 'array' binding will not work but we have to return something valid
        return OpenRpcParamStructure.Either;
    }
}
