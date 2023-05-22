using System.Collections;
using Json.Schema;
using Json.Schema.Generation;
using Microsoft.AspNetCore.Http;
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
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.OpenRpc;

public class OpenRpcDocumentGenerator : IOpenRpcDocumentGenerator
{
    private readonly IApiDescriptionGroupCollectionProvider apiDescriptionsProvider;
    private readonly IOpenRpcSchemaGenerator schemaGenerator;
    private readonly IOpenRpcContentDescriptorGenerator contentDescriptorGenerator;
    private readonly JsonRpcServerOptions serverOptions;
    private readonly OpenRpcOptions openRpcOptions;

    public OpenRpcDocumentGenerator(IApiDescriptionGroupCollectionProvider apiDescriptionsProvider, IOpenRpcSchemaGenerator schemaGenerator, IOpenRpcContentDescriptorGenerator contentDescriptorGenerator, IOptions<JsonRpcServerOptions> serverOptions, IOptions<OpenRpcOptions> openRpcOptions)
    {
        this.apiDescriptionsProvider = apiDescriptionsProvider;
        this.schemaGenerator = schemaGenerator;
        this.contentDescriptorGenerator = contentDescriptorGenerator;
        this.serverOptions = serverOptions.Value;
        this.openRpcOptions = openRpcOptions.Value;
    }

    public Models.OpenRpc Generate(Info info, string documentName, Uri host)
    {
        return new Models.OpenRpc
        {
            Openrpc = OpenRpcConstants.SpecVersion,
            Info = info,
            Servers = GetServers(host, serverOptions.RoutePrefix),
            Methods = GetMethods(documentName, host),
            Components = new()
            {
                Schemas = schemaGenerator.GetAllSchemas()
            }
        };
    }

    private List<Models.Server> GetServers(Uri host, string route)
    {
        var uriBuilder = new UriBuilder(host) { Path = route };
        var server = new Models.Server
        {
            Name = openRpcOptions.DefaultServerName,
            Url = uriBuilder.Uri
        };

        return new List<Models.Server>
        {
            server
        };
    }

    private List<Method> GetMethods(string documentName, Uri host) =>
        apiDescriptionsProvider.ApiDescriptionGroups.Items
            .SelectMany(static g => g.Items)
            .Where(d => !openRpcOptions.IgnoreObsoleteActions || d.IsObsoleteTransitive())
            .Where(static d => d.ActionDescriptor.EndpointMetadata.Any(static m => m is JsonRpcControllerAttribute))
            .Where(d => openRpcOptions.DocInclusionPredicate(documentName, d))
            .Select(d => GetMethod(d, host))
            .OrderBy(static m => m.Name)
            .ToList();

    private Method GetMethod(ApiDescription apiDescription, Uri host)
    {
        var methodInfo = (apiDescription.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
        var parametersMetadata = apiDescription.ActionDescriptor.EndpointMetadata.Get<JsonRpcActionParametersMetadata>();
        var serializerMetadata = apiDescription.ActionDescriptor.EndpointMetadata.Get<JsonRpcSerializerOptionsAttribute>();
        var serializerOptionsProviderType = serializerMetadata?.ProviderType;
        var methodName = (string) apiDescription.Properties[ApiExplorerConstants.MethodNameProperty];
        return new Method
        {
            Name = methodName,
            Summary = methodInfo?.GetXmlDocsSummary(),
            Description = methodInfo?.GetXmlDocsRemarks(),
            Params = GetMethodParams(apiDescription, methodName, parametersMetadata, serializerOptionsProviderType).ToList(),
            Result = GetResultContentDescriptor(apiDescription, methodName, serializerOptionsProviderType),
            Deprecated = apiDescription.IsObsoleteTransitive(),
            Servers = GetMethodServers(apiDescription, host),
            ParamStructure = GetParamsStructure(parametersMetadata)
        };
    }

    // ReSharper disable once UnusedParameter.Local
    private IEnumerable<ContentDescriptor> GetMethodParams(ApiDescription apiDescription, string methodName, JsonRpcActionParametersMetadata? parametersMetadata, Type? serializerOptionsProviderType)
    {
        // there must be only one body parameter with Request<T> type
        var requestType = apiDescription.ParameterDescriptions.Single(static x => x.Source == BindingSource.Body).Type;
        // unpack Request<T>
        var bodyType = requestType.BaseType!.GenericTypeArguments.Single();
        var isCollection = typeof(ICollection).IsAssignableFrom(bodyType);

        if (isCollection)
        {
            var itemType = bodyType.GetEnumerableItemType()!.ToContextualType();
            yield return contentDescriptorGenerator.GenerateForType(itemType, methodName, serializerOptionsProviderType);

            yield break;
        }

        foreach (var propertyInfo in bodyType.GetProperties())
        {
            if (parametersMetadata?.Parameters.TryGetValue(propertyInfo.Name, out var parameterMetadata) == true)
            {
                yield return contentDescriptorGenerator.GenerateForParameter(parameterMetadata.Type.ToContextualType(), methodName, serializerOptionsProviderType, parameterMetadata);
            }
            else
            {
                yield return contentDescriptorGenerator.GenerateForProperty(propertyInfo, methodName, serializerOptionsProviderType);
            }
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private ContentDescriptor GetResultContentDescriptor(ApiDescription apiDescription, string methodName, Type? serializerOptionsProviderType)
    {
        // there must be only one response type with Response<T> type
        var responseType = apiDescription.SupportedResponseTypes.Single().Type!;
        // unpack Response<T>
        var bodyType = responseType.BaseType!.GenericTypeArguments.Single().ToContextualType();
        return contentDescriptorGenerator.GenerateForType(bodyType, methodName, serializerOptionsProviderType);
    }

    private List<Models.Server>? GetMethodServers(ApiDescription apiDescription, Uri host)
    {
        var route = apiDescription.RelativePath?.Split('#').First();

        if (string.IsNullOrWhiteSpace(route) || $"/{route}" == serverOptions.RoutePrefix)
        {
            return null;
        }

        return GetServers(host, route);
    }

    private static ParamStructure GetParamsStructure(JsonRpcActionParametersMetadata? parametersMetadata)
    {
        var bindingStyles = parametersMetadata?.Parameters.Values
                .Select(static p => p.BindingStyle)
                .Distinct()
                .ToHashSet()
            ?? new HashSet<BindingStyle>();

        return bindingStyles.Count switch
        {
            0 => ParamStructure.Either,
            1 => bindingStyles.Single().ToParamStructure(),
            _ => CombineBindingStyles(bindingStyles)
        };
    }

    // mixed binding is bad but it's up to user to try this out
    private static ParamStructure CombineBindingStyles(IReadOnlySet<BindingStyle> bindingStyles)
    {
        var hasArrayBinding = bindingStyles.Contains(BindingStyle.Array);
        var hasObjectBinding = bindingStyles.Contains(BindingStyle.Object);
        if (hasArrayBinding && !hasObjectBinding)
        {
            // if 'array' is present, 'default' binding works, 'object' binding does not
            return ParamStructure.ByPosition;
        }

        if (hasObjectBinding && !hasArrayBinding)
        {
            // if 'object' is present, 'default' binding works, 'array' binding does not
            return ParamStructure.ByName;
        }

        // both 'object' and 'array' binding will not work but we have to return something valid
        return ParamStructure.Either;
    }
}
