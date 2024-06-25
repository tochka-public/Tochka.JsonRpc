using System.Collections;
using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using Json.Schema;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <inheritdoc />
[PublicAPI]
public class OpenRpcDocumentGenerator : IOpenRpcDocumentGenerator
{
    private readonly IApiDescriptionGroupCollectionProvider apiDescriptionsProvider;
    private readonly IOpenRpcSchemaGenerator schemaGenerator;
    private readonly IOpenRpcContentDescriptorGenerator contentDescriptorGenerator;
    private readonly IEnumerable<IJsonSerializerOptionsProvider> jsonSerializerOptionsProviders;
    private readonly JsonRpcServerOptions serverOptions;
    private readonly OpenRpcOptions openRpcOptions;
    private readonly ILogger<OpenRpcDocumentGenerator> log;

    /// <summary></summary>
    public OpenRpcDocumentGenerator(IApiDescriptionGroupCollectionProvider apiDescriptionsProvider, IOpenRpcSchemaGenerator schemaGenerator, IOpenRpcContentDescriptorGenerator contentDescriptorGenerator, IEnumerable<IJsonSerializerOptionsProvider> jsonSerializerOptionsProviders, IOptions<JsonRpcServerOptions> serverOptions, IOptions<OpenRpcOptions> openRpcOptions, ILogger<OpenRpcDocumentGenerator> log)
    {
        this.apiDescriptionsProvider = apiDescriptionsProvider;
        this.schemaGenerator = schemaGenerator;
        this.contentDescriptorGenerator = contentDescriptorGenerator;
        this.jsonSerializerOptionsProviders = jsonSerializerOptionsProviders;
        this.log = log;
        this.serverOptions = serverOptions.Value;
        this.openRpcOptions = openRpcOptions.Value;
    }

    /// <inheritdoc />
    public Models.OpenRpc Generate(OpenRpcInfo info, string documentName, Uri host)
    {
        var tags = GetControllersTags();
        return new(info)
        {
            Servers = GetServers(host, serverOptions.RoutePrefix.Value),
            Methods = GetMethods(documentName, host, tags),
            Components = new()
            {
                Schemas = schemaGenerator.GetAllSchemas(),
                Tags = tags
            }
        };
    }

    internal virtual Dictionary<string, OpenRpcTag> GetControllersTags()
    {
        var tags = apiDescriptionsProvider.ApiDescriptionGroups.Items
            .SelectMany(static g => g.Items)
            .Select(x => serverOptions.DefaultDataJsonSerializerOptions.ConvertName((x.ActionDescriptor as ControllerActionDescriptor)!.ControllerName))
            .Distinct();

        return tags.ToDictionary(static x => x, static x => new OpenRpcTag(x));
    }

    // internal virtual for mocking in tests
    internal virtual List<OpenRpcServer> GetServers(Uri host, string? route)
    {
        var uriBuilder = new UriBuilder(host) { Path = route };
        var server = new OpenRpcServer(openRpcOptions.DefaultServerName, uriBuilder.Uri);

        return new List<OpenRpcServer>
        {
            server
        };
    }

    // internal virtual for mocking in tests
    internal virtual List<OpenRpcMethod> GetMethods(string documentName, Uri host, Dictionary<string, OpenRpcTag> tags) =>
        apiDescriptionsProvider.ApiDescriptionGroups.Items
            .SelectMany(static g => g.Items)
            .Where(d => !openRpcOptions.IgnoreObsoleteActions || !IsObsoleteTransitive(d))
            .Where(static d => d.ActionDescriptor.EndpointMetadata.Any(static m => m is JsonRpcControllerAttribute))
            .Where(d => openRpcOptions.DocInclusionPredicate(documentName, d))
            .Select(d => GetMethod(d, host, tags))
            .OrderBy(static m => m.Name)
            .ToList();

    // internal virtual for mocking in tests
    internal virtual OpenRpcMethod GetMethod(ApiDescription apiDescription, Uri host, Dictionary<string, OpenRpcTag> tags)
    {
        var actionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
        var methodInfo = actionDescriptor?.MethodInfo;
        var controllerName = actionDescriptor?.ControllerName;
        var parametersMetadata = apiDescription.ActionDescriptor.EndpointMetadata.Get<JsonRpcActionParametersMetadata>();
        var serializerMetadata = apiDescription.ActionDescriptor.EndpointMetadata.Get<JsonRpcSerializerOptionsAttribute>();
        var jsonSerializerOptionsProviderType = serializerMetadata?.ProviderType;
        var jsonSerializerOptions = jsonSerializerOptionsProviderType == null
            ? serverOptions.DefaultDataJsonSerializerOptions
            : ServerUtils.GetJsonSerializerOptions(jsonSerializerOptionsProviders, jsonSerializerOptionsProviderType);
        var methodName = (string) apiDescription.Properties[ApiExplorerConstants.MethodNameProperty];
        return new OpenRpcMethod(methodName)
        {
            Summary = methodInfo?.GetXmlDocsSummary(),
            Description = methodInfo?.GetXmlDocsRemarks(),
            Params = GetMethodParams(apiDescription, methodName, parametersMetadata, jsonSerializerOptions).ToList(),
            Result = GetResultContentDescriptor(apiDescription, methodName, jsonSerializerOptions),
            Deprecated = IsObsoleteTransitive(apiDescription),
            Servers = GetMethodServers(apiDescription, host),
            ParamStructure = GetParamsStructure(parametersMetadata),
            Tags = GetMethodTags(controllerName, tags)
        };
    }

    internal virtual List<JsonSchema>? GetMethodTags(string? controllerName, Dictionary<string, OpenRpcTag> tags)
    {
        if (string.IsNullOrEmpty(controllerName))
        {
            return null;
        }

        controllerName = serverOptions.DefaultDataJsonSerializerOptions.ConvertName(controllerName);
        return tags.TryGetValue(controllerName, out _)
            ? new List<JsonSchema> { new JsonSchemaBuilder().Ref($"#/components/tags/{controllerName}").Build() }
            : null;
    }

    // internal virtual for mocking in tests
    internal virtual IEnumerable<OpenRpcContentDescriptor> GetMethodParams(ApiDescription apiDescription, string methodName, JsonRpcActionParametersMetadata? parametersMetadata, JsonSerializerOptions jsonSerializerOptions)
    {
        // there must be only one body parameter with Request<T> type or inherited from it
        var requestType = apiDescription.ParameterDescriptions.Single(static x => x.Source == BindingSource.Body).Type;
        // unpack Request<T>
        var bodyType = requestType.BaseType!.GenericTypeArguments.FirstOrDefault() ?? requestType.GenericTypeArguments.Single();

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
                yield return contentDescriptorGenerator.GenerateForParameter(property, methodName, parameterMetadata, jsonSerializerOptions);
            }
            else
            {
                yield return contentDescriptorGenerator.GenerateForProperty(property, methodName, jsonSerializerOptions);
            }
        }
    }

    // internal virtual for mocking in tests
    internal virtual OpenRpcContentDescriptor GetResultContentDescriptor(ApiDescription apiDescription, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        // there must be only one response type with Response<T> type or inherited from it
        var responseType = apiDescription.SupportedResponseTypes.Single().Type!;
        // unpack Response<T>
        var bodyType = responseType.BaseType!.GenericTypeArguments.FirstOrDefault() ?? responseType.GenericTypeArguments.Single();
        return contentDescriptorGenerator.GenerateForType(bodyType.ToContextualType(), methodName, jsonSerializerOptions);
    }

    // internal virtual for mocking in tests
    internal virtual List<OpenRpcServer>? GetMethodServers(ApiDescription apiDescription, Uri host)
    {
        var route = apiDescription.RelativePath?.Split('#').First();

        if (string.IsNullOrWhiteSpace(route) || $"/{route}" == serverOptions.RoutePrefix)
        {
            return null;
        }

        return GetServers(host, route);
    }

    // internal virtual for mocking in tests
    internal virtual OpenRpcParamStructure GetParamsStructure(JsonRpcActionParametersMetadata? parametersMetadata)
    {
        var bindingStyles = parametersMetadata?.Parameters.Values
                .Select(static p => p.BindingStyle)
                .Distinct()
                .ToHashSet()
            ?? new HashSet<BindingStyle>();

        return bindingStyles.Count switch
        {
            0 => OpenRpcParamStructure.Either,
            1 => ToParamStructure(bindingStyles.Single()),
            _ => CombineBindingStyles(bindingStyles)
        };
    }

    // mixed binding is bad but it's up to user to try this out
    // internal virtual for mocking in tests
    internal virtual OpenRpcParamStructure CombineBindingStyles(IReadOnlySet<BindingStyle> bindingStyles)
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
        log.LogWarning("Both Array and Object binding styles in method params - this will work unexpectedly");
        return OpenRpcParamStructure.Either;
    }

    private static bool IsObsoleteTransitive(ApiDescription description)
    {
        var methodInfo = (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
        var methodAttr = methodInfo?.GetCustomAttribute<ObsoleteAttribute>();
        var typeAttr = methodInfo?.DeclaringType?.GetCustomAttribute<ObsoleteAttribute>();
        return (methodAttr ?? typeAttr) != null;
    }

    private static OpenRpcParamStructure ToParamStructure(BindingStyle bindingStyle) => bindingStyle switch
    {
        BindingStyle.Default => OpenRpcParamStructure.Either,
        BindingStyle.Object => OpenRpcParamStructure.ByName,
        BindingStyle.Array => OpenRpcParamStructure.ByPosition,
        _ => throw new ArgumentOutOfRangeException(nameof(bindingStyle), bindingStyle, null)
    };
}
