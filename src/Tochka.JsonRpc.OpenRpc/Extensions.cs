using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.OpenRpc.Services;

namespace Tochka.JsonRpc.OpenRpc;

/// <summary>
/// Extensions to configure using OpenRPC in application
/// </summary>
[PublicAPI]
public static class Extensions
{
    /// <summary>
    /// Register services required for OpenRPC document generation and configure OpenRPC options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to</param>
    /// <param name="xmlDocAssembly">Assembly with xml documentation for API methods and model types</param>
    /// <param name="info">Metadata about API</param>
    /// <param name="setupAction">Delegate used to configure OpenRPC options</param>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly, OpenRpcInfo info, Action<OpenRpcOptions> setupAction)
    {
        services.TryAddScoped<IOpenRpcDocumentGenerator, OpenRpcDocumentGenerator>();
        services.TryAddScoped<IOpenRpcSchemaGenerator, OpenRpcSchemaGenerator>();
        services.TryAddScoped<IOpenRpcContentDescriptorGenerator, OpenRpcContentDescriptorGenerator>();
        services.TryAddSingleton<ITypeEmitter, TypeEmitter>();
        if (services.All(static x => x.ImplementationType != typeof(JsonRpcDescriptionProvider)))
        {
            // add by interface if not present
            services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();
        }

        services.Configure<OpenRpcOptions>(c =>
        {
            setupAction(c);
            c.OpenRpcDoc(ApiExplorerConstants.DefaultDocumentName, info);
        });

        var xmlFile = $"{xmlDocAssembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (!File.Exists(xmlPath))
        {
            // check to enforce users set up their projects properly
            throw new FileNotFoundException("OpenRpc requires generated XML doc file! Add <GenerateDocumentationFile>true</GenerateDocumentationFile> to your csproj or disable OpenRpc integration", xmlPath);
        }

        services.AddSingleton<OpenRpcMarkerService>();

        return services;
    }

    /// <summary>
    /// Register services required for OpenRPC document generation with metadata from Assembly and configure OpenRPC options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to</param>
    /// <param name="xmlDocAssembly">Assembly with xml documentation for API methods and model types</param>
    /// <param name="setupAction">Delegate used to configure OpenRPC options</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly, Action<OpenRpcOptions> setupAction)
    {
        // returns assembly name, not what Rider shows in Csproj>Properties>Nuget>Title
        var assemblyName = xmlDocAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var title = $"{assemblyName} {ApiExplorerConstants.DefaultDocumentTitle}".TrimStart();
        var description = xmlDocAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        var info = new OpenRpcInfo(title, ApiExplorerConstants.DefaultDocumentVersion)
        {
            Description = description
        };

        return services.AddOpenRpc(xmlDocAssembly, info, setupAction);
    }

    /// <summary>
    /// Register services required for OpenRPC document generation
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to</param>
    /// <param name="xmlDocAssembly">Assembly with xml documentation for API methods and model types</param>
    /// <param name="info">Metadata about API</param>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    [ExcludeFromCodeCoverage]
    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly, OpenRpcInfo info) =>
        services.AddOpenRpc(xmlDocAssembly, info, static _ => { });

    /// <summary>
    /// Register services required for OpenRPC document generation with metadata from Assembly
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to</param>
    /// <param name="xmlDocAssembly">Assembly with xml documentation for API methods and model types</param>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    [ExcludeFromCodeCoverage]
    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly) =>
        services.AddOpenRpc(xmlDocAssembly, static _ => { });

    /// <summary>
    /// Define document to be created by OpenRPC generator
    /// </summary>
    /// <param name="options">OpenRPC options to add document definition to</param>
    /// <param name="name">A URI-friendly name that uniquely identifies the document</param>
    /// <param name="info">Metadata about API</param>
    public static void OpenRpcDoc(this OpenRpcOptions options, string name, OpenRpcInfo info) =>
        options.Docs[name] = info;

    /// <summary>
    /// Use middleware to generate OpenRPC document
    /// </summary>
    /// <param name="app">Application to add middleware to</param>
    /// <exception cref="InvalidOperationException">If AddOpenRpc was not called before</exception>
    [ExcludeFromCodeCoverage(Justification = "it's almost impossible to test UseMiddleware")]
    public static IApplicationBuilder UseOpenRpc(this IApplicationBuilder app)
    {
        EnsureRequiredServicesRegistered(app.ApplicationServices);
        return app.UseMiddleware<OpenRpcMiddleware>();
    }

    /// <summary>
    /// Map requests to generate OpenRPC document
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder" /></param>
    [ExcludeFromCodeCoverage(Justification = "MapGet almost impossible to test")]
    public static IEndpointConventionBuilder MapOpenRpc(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<OpenRpcOptions>>().Value;
        var requestDelegate = endpoints.CreateApplicationBuilder().UseOpenRpc().Build();
        return endpoints.MapGet(options.DocumentPath, requestDelegate);
    }

    private static void EnsureRequiredServicesRegistered(IServiceProvider services)
    {
        if (services.GetService<OpenRpcMarkerService>() == null)
        {
            throw new InvalidOperationException($"Unable to find the required services. Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(AddOpenRpc)}' in the application startup code.");
        }
    }
}
