using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Server.Serialization;

namespace Tochka.JsonRpc.Swagger;

/// <inheritdoc />
/// <summary>
/// Swagger options configuration. Required to access services from DI during configuration
/// </summary>
internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly Assembly assembly;
    private readonly IApiVersionDescriptionProvider provider;
    private readonly IEnumerable<IJsonSerializerOptionsProvider> jsonSerializerOptionsProviders;

    public ConfigureSwaggerOptions(Assembly assembly, IApiVersionDescriptionProvider provider, IEnumerable<IJsonSerializerOptionsProvider> jsonSerializerOptionsProviders)
    {
        this.assembly = assembly;
        this.provider = provider;
        this.jsonSerializerOptionsProviders = jsonSerializerOptionsProviders;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // returns assembly name, not what Rider shows in Csproj>Properties>Nuget>Title
        var assemblyName = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var title = $"{assemblyName} {ApiExplorerConstants.DefaultDocumentTitle}".TrimStart();
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        var jsonSerializerOptionProviderTypes = jsonSerializerOptionsProviders
            .Select(static p => p.GetType())
            .ToList();
        foreach (var versionDescription in provider.ApiVersionDescriptions)
        {
            var info = new OpenApiInfo
            {
                Title = title,
                Version = versionDescription.ApiVersion.ToString(),
                Description = description
            };
            options.JsonRpcSwaggerDocs(ApiExplorerConstants.DefaultDocumentName, $"_{versionDescription.GroupName}", info, jsonSerializerOptionProviderTypes);
        }
    }
}
