using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc;

/// <inheritdoc />
/// <summary>
/// OpenRpc options configuration. Required to access services from DI during configuration
/// </summary>
internal class ConfigureOpenRpcOptions : IConfigureOptions<OpenRpcOptions>
{
    private readonly Assembly assembly;
    private readonly IApiVersionDescriptionProvider provider;

    public ConfigureOpenRpcOptions(Assembly assembly, IApiVersionDescriptionProvider provider)
    {
        this.assembly = assembly;
        this.provider = provider;
    }

    public void Configure(OpenRpcOptions options)
    {
        // returns assembly name, not what Rider shows in Csproj>Properties>Nuget>Title
        var assemblyName = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var title = $"{assemblyName} {ApiExplorerConstants.DefaultDocumentTitle}".TrimStart();
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        foreach (var versionDescription in provider.ApiVersionDescriptions)
        {
            var info = new OpenRpcInfo(title, versionDescription.ApiVersion.ToString())
            {
                Description = description
            };
            options.OpenRpcDoc($"{ApiExplorerConstants.DefaultDocumentName}_{versionDescription.GroupName}", info);
        }
    }
}
