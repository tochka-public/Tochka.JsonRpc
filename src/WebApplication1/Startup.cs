using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server;
using WebApplication1.Controllers;
using WebApplication1.Services;
using SchemaGenerator = WebApplication1.Services.SchemaGenerator;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonRpcServer()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddTransient<IApiDescriptionProvider, WtfProvider>();  // custom
            services.AddSwaggerGen(options =>
            {
                
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "aaa", Version = "v1" });
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Request<>).Assembly.GetName().Name}.xml"));
                // TODO add doc for current application assembly?
                
                // TODO how to use custom json serializer?
                options.SchemaFilter<WtfFilter>();  // custom
                //options.RequestBodyFilter<BodyFilter>();

            });
            services.Replace(ServiceDescriptor.Transient<ISchemaGenerator, SchemaGenerator>());
            services.TryAddJsonRpcSerializer<CamelCaseJsonRpcSerializer>();
            services.AddSwaggerGenNewtonsoftSupport();
            //services.AddSwaggerGenJsonRpcSupport();  // custom
            services.AddSingleton<ITypeEmitter, TypeEmitter>();
        }
        

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseMvc();
        }
    }
}
