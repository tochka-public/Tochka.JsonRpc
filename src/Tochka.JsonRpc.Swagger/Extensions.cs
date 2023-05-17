using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.Swagger;

public static class Extensions
{
    public static IServiceCollection AddSwaggerWithJsonRpc(this IServiceCollection services, Assembly xmlDocAssembly, OpenApiInfo info)
    {
        services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();
        services.TryAddTransient<ISchemaGenerator, JsonRpcSchemaGenerator>();
        services.TryAddSingleton<ITypeEmitter, TypeEmitter>();
        services.AddSwaggerGen(c =>
        {
            // it's impossible to add same model with different serializers, so we have to create separate documents for each serializer
            c.SwaggerDoc(Constants.DefaultDocumentName, info);
            var jsonSerializerOptionsProviders = services
                .Where(static x => typeof(IJsonSerializerOptionsProvider).IsAssignableFrom(x.ServiceType))
                .Select(static x => x.ImplementationType)
                .Where(static x => x != null)
                .Distinct();
            foreach (var providerType in jsonSerializerOptionsProviders)
            {
                var documentName = Utils.GetDocumentName(providerType);
                var documentInfo = new OpenApiInfo
                {
                    Title = info.Title,
                    Version = info.Version,
                    Description = $"Serializer: {providerType!.Name}\n{info.Description}",
                    Contact = info.Contact,
                    License = info.License,
                    TermsOfService = info.TermsOfService,
                    Extensions = info.Extensions
                };
                c.SwaggerDoc(documentName, documentInfo);
            }

            c.DocInclusionPredicate(DocumentSelector);
            c.SchemaFilter<JsonRpcPropertiesFilter>();

            // to correctly create request and response models with controller.action binding style
            c.CustomSchemaIds(static t => t.Assembly.FullName?.StartsWith(Constants.GeneratedModelsAssemblyId, StringComparison.Ordinal) == true
                ? t.FullName
                : t.Name);

            var xmlFile = $"{xmlDocAssembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (!File.Exists(xmlPath))
            {
                // check to enforce users set up their projects properly
                throw new FileNotFoundException("Swagger requires generated XML doc file! Add <GenerateDocumentationFile>true</GenerateDocumentationFile> to your csproj or disable Swagger integration", xmlPath);
            }

            c.IncludeXmlComments(xmlPath);
            c.SupportNonNullableReferenceTypes();
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJsonRpc(this IServiceCollection services, Assembly xmlDocAssembly) =>
        services.AddSwaggerWithJsonRpc(xmlDocAssembly, new OpenApiInfo { Title = Constants.DefaultDocumentTitle, Version = Constants.DefaultDocumentVersion });

    public static void JsonRpcSwaggerEndpoints(this SwaggerUIOptions options, IApplicationBuilder app, string name)
    {
        options.SwaggerEndpoint(GetSwaggerDocumentUrl(Constants.DefaultDocumentName), name);
        var jsonSerializerOptionsProviders = app.ApplicationServices.GetRequiredService<IEnumerable<IJsonSerializerOptionsProvider>>();
        foreach (var provider in jsonSerializerOptionsProviders)
        {
            var documentName = Utils.GetDocumentName(provider.GetType());
            options.SwaggerEndpoint(GetSwaggerDocumentUrl(documentName), $"{name} {GetSwaggerEndpointSuffix(provider)}");
        }
    }

    public static void JsonRpcSwaggerEndpoints(this SwaggerUIOptions options, IApplicationBuilder app) =>
        options.JsonRpcSwaggerEndpoints(app, Constants.DefaultDocumentTitle);

    private static bool DocumentSelector(string docName, ApiDescription description)
    {
        if (docName.StartsWith(Constants.DefaultDocumentName, StringComparison.Ordinal))
        {
            return description.GroupName == docName;
        }

        return description.GroupName == null || description.GroupName == docName;
    }

    private static string GetSwaggerDocumentUrl(string docName) => $"/swagger/{docName}/swagger.json";

    private static string GetSwaggerEndpointSuffix(IJsonSerializerOptionsProvider jsonSerializerOptionsProvider)
    {
        var caseName = jsonSerializerOptionsProvider.GetType().Name.Replace(nameof(IJsonSerializerOptionsProvider)[1..], "");
        return jsonSerializerOptionsProvider.Options.PropertyNamingPolicy?.ConvertName(caseName) ?? caseName;
    }
}
