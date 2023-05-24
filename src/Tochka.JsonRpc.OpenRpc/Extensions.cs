using System.Reflection;
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

public static class Extensions
{
    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly, OpenRpcInfo info, Action<OpenRpcOptions> setupAction)
    {
        services.TryAddTransient<IOpenRpcDocumentGenerator, OpenRpcDocumentGenerator>();
        services.TryAddTransient<IOpenRpcSchemaGenerator, OpenRpcSchemaGenerator>();
        services.TryAddTransient<IOpenRpcContentDescriptorGenerator, OpenRpcContentDescriptorGenerator>();
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
            throw new FileNotFoundException("Swagger requires generated XML doc file! Add <GenerateDocumentationFile>true</GenerateDocumentationFile> to your csproj or disable Swagger integration", xmlPath);
        }

        services.AddSingleton<OpenRpcMarkerService>();

        return services;
    }

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

    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly, OpenRpcInfo info) =>
        services.AddOpenRpc(xmlDocAssembly, info, static _ => { });

    public static IServiceCollection AddOpenRpc(this IServiceCollection services, Assembly xmlDocAssembly) =>
        services.AddOpenRpc(xmlDocAssembly, static _ => { });

    public static void OpenRpcDoc(this OpenRpcOptions options, string name, OpenRpcInfo info) =>
        options.Docs[name] = info;

    public static IApplicationBuilder UseOpenRpc(this IApplicationBuilder app)
    {
        EnsureRequiredServicesRegistered(app.ApplicationServices);
        return app.UseMiddleware<OpenRpcMiddleware>();
    }

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
