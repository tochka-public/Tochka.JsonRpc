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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Server;
using WebApplication1.Controllers;

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
            //services.AddTransient<IApiDescriptionProvider, WtfProvider>();
            services.AddTransient<IApiDescriptionProvider, KostylProvider>();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "aaa", Version = "v1" });
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Request<>).Assembly.GetName().Name}.xml"));
                // TODO add doc for current application assembly?
                
                /* this works but custom schema insertion should be better...
                 options.MapType<IRpcId>(() => new OpenApiSchema()
                {
                    Type = "string",
                    Example = new OpenApiString("1")
                });
                */
                // TODO how to use custom json serializer?
                options.SchemaFilter<WtfFilter>();
                
            });
            services.AddSwaggerGenNewtonsoftSupport();
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
