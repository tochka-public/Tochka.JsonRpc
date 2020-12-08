using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Swagger
{
    public static class Extensions
    {
        /// <summary>
        /// Adds different documents for different JsonRpc serializers,
        /// because Swagger Schema for a type can only be created once.
        /// You probably don't want to mix snake/camel case anyway in one API.
        /// Also adds a default document for REST
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerWithJsonRpc(this IServiceCollection services)
        {
            // TODO add options lambda
            services.TryAddSingleton<ITypeEmitter, TypeEmitter>();
            if (services.All(x => x.ImplementationType != typeof(JsonRpcDescriptionProvider)))
            {
                services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();
            }

            services.TryAddTransient<ISchemaGenerator, SchemaGeneratorWrapper>();

            services.AddSwaggerGen(options =>
            {
                options.SchemaFilter<CommonJsonRpcPropertiesFilter>();
                options.DocInclusionPredicate(DocumentSelector);
                // TODO add options to skip this if user wants to customize?
                options.SwaggerDoc(JsonRpcConstants.DefaultSwaggerDoc, new OpenApiInfo {Title = $"{JsonRpcConstants.DefaultSwaggerDoc}", Version = "v1"});

                var registeredSerializers = services
                    .Where(x =>
                        x.Lifetime == ServiceLifetime.Singleton
                        && typeof(IJsonRpcSerializer).IsAssignableFrom(x.ServiceType)
                    )
                    .Select(x => x.ImplementationType)
                    .Distinct()
                    .ToHashSet();
                // skipping this because no API should be actually using it
                registeredSerializers.Remove(typeof(HeaderJsonRpcSerializer));
                foreach (var serializerType in registeredSerializers)
                {
                    var name = Utils.GetSwaggerFriendlyDocumentName(serializerType, DefaultSerializer);
                    options.SwaggerDoc(name, new OpenApiInfo {Title = name, Version = "v1"});
                }

                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

            });
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddSingleton<IStartupFilter, SwaggerUiWithJsonRpcStartupFilter>();
            return services;
        }

        private static bool DocumentSelector(string docName, ApiDescription description)
        {
            if (description.GroupName == null)
            {
                return docName.Equals(JsonRpcConstants.DefaultSwaggerDoc, StringComparison.InvariantCultureIgnoreCase);
            }

            if (description.GroupName.StartsWith(JsonRpcConstants.ApiDocumentName, StringComparison.InvariantCultureIgnoreCase))
            {
                return docName.Equals(description.GroupName, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        private static readonly Type DefaultSerializer = typeof(SnakeCaseJsonRpcSerializer);

        public static IApplicationBuilder UseSwaggerUiWithJsonRpc(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{JsonRpcConstants.DefaultSwaggerDoc}/swagger.json", $"{JsonRpcConstants.DefaultSwaggerDoc}");

                var serializerTypes = app.ApplicationServices
                    .GetRequiredService<IEnumerable<IJsonRpcSerializer>>()
                    .Select(x => x.GetType());
                foreach (var serializerType in serializerTypes)
                {
                    // skip this because no API should be actually using it
                    if (serializerType == typeof(HeaderJsonRpcSerializer))
                    {
                        continue;
                    }

                    var name = Utils.GetSwaggerFriendlyDocumentName(serializerType, DefaultSerializer);
                    c.SwaggerEndpoint($"/swagger/{name}/swagger.json", $"{name}");
                }
            });

            return app;
        }
    }
}