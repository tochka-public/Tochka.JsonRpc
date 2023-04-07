using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tochka.JsonRpc.Server;

/// <inheritdoc />
/// <summary>
/// Helper to enable DI in conventions.
/// See https://github.com/aspnet/Mvc/issues/6214
/// and https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0#use-di-services-to-configure-options
/// </summary>
[ExcludeFromCodeCoverage]
internal class ConventionConfigurator<TConvention> : IConfigureOptions<MvcOptions>
{
    public ConventionConfigurator(TConvention convention) => this.convention = convention;

    private readonly TConvention convention;

    public void Configure(MvcOptions options)
    {
        switch (convention)
        {
            case IApplicationModelConvention x:
                options.Conventions.Add(x);
                return;
            case IControllerModelConvention x:
                options.Conventions.Add(x);
                return;
            case IActionModelConvention x:
                options.Conventions.Add(x);
                return;
            case IParameterModelConvention x:
                options.Conventions.Add(x);
                return;
            default:
                throw new InvalidOperationException($"Unsupported convention type: {typeof(TConvention)}");
        }
    }
}
