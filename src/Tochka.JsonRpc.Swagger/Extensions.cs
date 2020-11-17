using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Serializers;

namespace WebApplication1.Services
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
        public static IServiceCollection AddSwaggerGenWithJsonRpc(this IServiceCollection services)
        {
            services.AddSingleton<ITypeEmitter, TypeEmitter>();
            services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();
            services.AddTransient<ISchemaGenerator, SchemaGeneratorWrapper>();

            services.AddSwaggerGen(options =>
            {
                options.SchemaFilter<CommonPropertiesFilter>();
                options.DocInclusionPredicate(DocumentSelector);
                // TODO add options to skip this if user wants to customize?
                options.SwaggerDoc(JsonRpcConstants.DefaultSwaggerDoc, new OpenApiInfo { Title = $"{JsonRpcConstants.DefaultSwaggerDoc}", Version = "v1" });

                var registeredSerializers = services
                    .Where(x => 
                        x.Lifetime == ServiceLifetime.Singleton 
                        && x.ServiceType.IsAssignableTo(typeof(IJsonRpcSerializer))
                        )
                    .Select(x => x.ImplementationType)
                    .Distinct();
                foreach (var serializerType in registeredSerializers)
                {
                    // skip this because no API should be actually using it
                    if (serializerType == typeof(HeaderJsonRpcSerializer))
                    {
                        continue;
                    }

                    var name = Utils.GetSwaggerFriendlyDocumentName(serializerType);
                    options.SwaggerDoc(name, new OpenApiInfo { Title = name, Version = "v1" });
                }
                // TODO add doc for current application assembly by default? for all user assemblies? how?
                // TODO add options lambda
            });
            services.AddSwaggerGenNewtonsoftSupport();
            return services;
        }

        private static bool DocumentSelector(string docName, ApiDescription description)
        {
            if (description.GroupName == null)
            {
                return docName.Equals(JsonRpcConstants.DefaultSwaggerDoc, StringComparison.InvariantCultureIgnoreCase);
            }

            if (description.GroupName.StartsWith(JsonRpcConstants.JsonRpcSwaggerPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return docName.Equals(description.GroupName, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

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

                    var name = Utils.GetSwaggerFriendlyDocumentName(serializerType);
                    c.SwaggerEndpoint($"/swagger/{name}/swagger.json", $"{name}");
                }
            });

            return app;
        }

    }
}