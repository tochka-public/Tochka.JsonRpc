using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tochka.JsonRpc.V1.Server.Conventions
{
    /// <summary>
    /// Helper to enable DI in conventions.
    /// See https://github.com/aspnet/Mvc/issues/6214
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ExcludeFromCodeCoverage]
    internal class ConventionConfigurator<T> : IConfigureOptions<MvcOptions>
    {
        public ConventionConfigurator(T convention)
        {
            Convention = convention;
        }

        public T Convention { get; set; }

        public void Configure(MvcOptions options)
        {
            switch (Convention)
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
                    throw new InvalidOperationException("Unsupported convention type");
            }
        }
    }
}