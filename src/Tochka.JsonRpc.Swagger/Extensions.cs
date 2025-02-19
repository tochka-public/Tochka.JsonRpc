using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.Swagger;

/// <summary>
/// Extensions to configure using Swagger for JSON-RPC in application
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Register services required for JSON-RPC Swagger document generation and configure Swagger options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to</param>
    /// <param name="xmlDocAssembly">Assembly with xml documentation for API methods and model types</param>
    /// <param name="setupAction">Delegate used to additionally configure Swagger options</param>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    /// <remarks>
    /// This method doesn't register SwaggerDocs - You need to add it manually in <paramref name="setupAction" />
    /// using <see cref="JsonRpcSwaggerDocs" /> or <see cref="SwaggerGenOptionsExtensions.SwaggerDoc" />.<br />
    /// You can use overload without <paramref name="setupAction" /> - it will register docs for all versions and serializers
    /// </remarks>
    public static IServiceCollection AddSwaggerWithJsonRpc(this IServiceCollection services, Assembly xmlDocAssembly, Action<SwaggerGenOptions> setupAction)
    {
        services.TryAddTransient<ISchemaGenerator, JsonRpcSchemaGenerator>();
        services.TryAddSingleton<ITypeEmitter, TypeEmitter>();
        if (services.All(static x => x.ImplementationType != typeof(JsonRpcDescriptionProvider)))
        {
            // add by interface if not present
            services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();
        }

        services.AddSwaggerGen(c =>
        {
            setupAction(c);

            c.DocInclusionPredicate(DocumentSelector);
            c.SchemaFilter<JsonRpcPropertiesFilter>();

            // to correctly create request and response models with controller.action binding style
            c.CustomSchemaIds(SchemaIdSelector);

            var xmlFile = $"{xmlDocAssembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (!File.Exists(xmlPath))
            {
                // check to enforce users set up their projects properly
                throw new FileNotFoundException("Swagger requires generated XML doc file! Add <GenerateDocumentationFile>true</GenerateDocumentationFile> to your csproj or disable Swagger integration", xmlPath);
            }

            c.IncludeXmlComments(xmlPath);
            c.SupportNonNullableReferenceTypes();

            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2505
            c.MapType<TimeSpan>(() => new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("00:00:00")
            });
        });

        return services;
    }

    /// <summary>
    /// Register services required for JSON-RPC Swagger document generation and SwaggerDocs for all versions and serializers with metadata from Assembly
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to</param>
    /// <param name="xmlDocAssembly">Assembly with xml documentation for API methods and model types</param>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    /// <remarks>
    /// This method registers SwaggerDocs for all versions and serializers with title and description from Assembly.<br />
    /// If you want to add docs and <see cref="OpenApiInfo" /> for them manually - use overload with setup action
    /// </remarks>
    public static IServiceCollection AddSwaggerWithJsonRpc(this IServiceCollection services, Assembly xmlDocAssembly)
    {
        // we need to get IApiVersionDescriptionProvider and IJsonSerializerOptionsProviders from DI so can't use Configure<>() here
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>(x =>
            new ConfigureSwaggerOptions(xmlDocAssembly, x.GetRequiredService<IApiVersionDescriptionProvider>(), x.GetServices<IJsonSerializerOptionsProvider>()));

        return services.AddSwaggerWithJsonRpc(xmlDocAssembly, static _ => { });
    }

    /// <summary>
    /// Register SwaggerDocs for default serializer and all custom serializers
    /// </summary>
    /// <param name="options">SwaggerGenOptions options to add docs to</param>
    /// <param name="namePrefix">Prefix of document name. Custom serializers' names will be added after this prefix with '_' delimiter</param>
    /// <param name="nameSuffix">Suffix of document name. Will be added to documents' names without delimiter</param>
    /// <param name="info">Metadata about API</param>
    /// <param name="jsonSerializerOptionsProviderTypes">Custom serializer options providers to register docs for</param>
    public static void JsonRpcSwaggerDocs(this SwaggerGenOptions options, string namePrefix, string nameSuffix, OpenApiInfo info, IEnumerable<Type> jsonSerializerOptionsProviderTypes)
    {
        // it's impossible to add same model with different serializers, so we have to create separate documents for each serializer
        options.SwaggerDoc($"{namePrefix}{nameSuffix}", info);

        foreach (var providerType in jsonSerializerOptionsProviderTypes)
        {
            var documentName = ApiExplorerUtils.GetDocumentName(namePrefix, providerType);
            var documentInfo = new OpenApiInfo
            {
                Title = info.Title,
                Version = info.Version,
                Description = $"Serializer: {providerType.Name}\n{info.Description}",
                Contact = info.Contact,
                License = info.License,
                TermsOfService = info.TermsOfService,
                Extensions = info.Extensions
            };
            options.SwaggerDoc($"{documentName}{nameSuffix}", documentInfo);
        }
    }

    /// <summary>
    /// Add Swagger JSON endpoints for all versions and serializers of JSON-RPC API
    /// </summary>
    /// <param name="options">SwaggerUI options to add endpoints to</param>
    /// <param name="services">The <see cref="IServiceProvider" /> to retrieve serializer options providers</param>
    /// <param name="name">Name that appears in the document selector drop-down</param>
    /// <remarks>
    /// Adds endpoint for actions with default serializer options and additional endpoints for every <see cref="IJsonSerializerOptionsProvider" />
    /// </remarks>
    public static void JsonRpcSwaggerEndpoints(this SwaggerUIOptions options, IServiceProvider services, string name)
    {
        var versionDescriptionProvider = services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var versionDescription in versionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(GetSwaggerDocumentUrl($"{ApiExplorerConstants.DefaultDocumentName}_{versionDescription.GroupName}"), $"{name} {versionDescription.ApiVersion}");
            var jsonSerializerOptionsProviders = services.GetRequiredService<IEnumerable<IJsonSerializerOptionsProvider>>();
            foreach (var provider in jsonSerializerOptionsProviders)
            {
                var documentName = ApiExplorerUtils.GetDocumentName(ApiExplorerConstants.DefaultDocumentName, provider.GetType());
                options.SwaggerEndpoint(GetSwaggerDocumentUrl($"{documentName}_{versionDescription.GroupName}"), $"{name} {GetSwaggerEndpointSuffix(provider)} {versionDescription.ApiVersion}");
            }
        }
    }

    /// <summary>
    /// Add Swagger JSON endpoints for all versions and serializers of JSON-RPC API with default name (<see cref="ApiExplorerConstants.DefaultDocumentTitle" />)
    /// </summary>
    /// <param name="options">SwaggerUI options to add endpoints to</param>
    /// <param name="services">The <see cref="IServiceProvider" /> to retrieve serializer options providers</param>
    /// <remarks>
    /// Adds endpoint for actions with default serializer options and additional endpoints for every <see cref="IJsonSerializerOptionsProvider" />
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static void JsonRpcSwaggerEndpoints(this SwaggerUIOptions options, IServiceProvider services) =>
        options.JsonRpcSwaggerEndpoints(services, ApiExplorerConstants.DefaultDocumentTitle);

    // internal for tests
    internal static bool DocumentSelector(string docName, ApiDescription description)
    {
        if (docName.StartsWith(ApiExplorerConstants.DefaultDocumentName, StringComparison.Ordinal))
        {
            return description.GroupName == docName;
        }

        return description.GroupName == null || description.GroupName == docName;
    }

    // internal for tests
    internal static string SchemaIdSelector(Type type) =>
        type.Assembly.FullName?.StartsWith(ApiExplorerConstants.GeneratedModelsAssemblyName, StringComparison.Ordinal) == true
            ? type.FullName!
            : type.Name;

    [ExcludeFromCodeCoverage]
    private static string GetSwaggerDocumentUrl(string docName) => $"/swagger/{docName}/swagger.json";

    [ExcludeFromCodeCoverage]
    private static string GetSwaggerEndpointSuffix(IJsonSerializerOptionsProvider jsonSerializerOptionsProvider)
    {
        var caseName = jsonSerializerOptionsProvider.GetType().Name.Replace(nameof(IJsonSerializerOptionsProvider)[1..], "");
        return jsonSerializerOptionsProvider.Options.ConvertName(caseName);
    }
}
