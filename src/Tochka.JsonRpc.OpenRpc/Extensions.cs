using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using JetBrains.Annotations;
using Json.Schema;
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
    /// <param name="setupAction">Delegate used to configure OpenRPC options</param>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    /// <remarks>
    /// This method doesn't register OpenRpcDocs - You need to add it manually in <paramref name="setupAction" />
    /// using <see cref="OpenRpcDoc" />.<br />
    /// You can use overload without <paramref name="setupAction" /> - it will register docs for all versions
    /// </remarks>
    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly, Action<OpenRpcOptions> setupAction)
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

        services.Configure(setupAction);

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
    /// Register services required for OpenRPC document generation and OpenRpcDocs for all versions with metadata from Assembly
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to</param>
    /// <param name="xmlDocAssembly">Assembly with xml documentation for API methods and model types</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">If xml documentation disabled</exception>
    /// <remarks>
    /// This method registers OpenRpcDocs for all versions with title and description from Assembly.<br />
    /// If you want to add docs and <see cref="OpenRpcInfo" /> for them manually - use overload with setup action
    /// </remarks>
    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly)
    {
        // we need to get IApiVersionDescriptionProvider from DI so can't use Configure<>() here
        services.AddTransient<IConfigureOptions<OpenRpcOptions>, ConfigureOpenRpcOptions>(x =>
            new ConfigureOpenRpcOptions(xmlDocAssembly, x.GetRequiredService<IApiVersionDescriptionProvider>()));

        return services.AddOpenRpc(xmlDocAssembly, static _ => { });
    }

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

    public static JsonSchema BuildWithoutUri(this JsonSchemaBuilder builder)
    {
        var result = builder.Build();
        result.BaseUri = null!;
        return result;
    }
}
