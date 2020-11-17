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
using Tochka.JsonRpc.OpenRpc;
using Tochka.JsonRpc.Server;
using WebApplication1.Controllers;
using WebApplication1.Services;

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
                .AddJsonRpcServer(options =>
                {
                    options.DetailedResponseExceptions = true;
                    options.AllowRawResponses = true;

                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSwaggerGenWithJsonRpc();
            services.TryAddJsonRpcSerializer<CamelCaseJsonRpcSerializer>();

            services.AddTransient<ContentDescriptorGenerator>();
            services.AddTransient<OpenRpcGenerator>();
            services.Configure<OpenRpcOptions>(options =>
            {
                options.Docs.Add("test", new OpenApiInfo()
                {
                    Description = "alala",
                    Title = "title",
                    Version = "42"
                });
            });
        }
        

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // TODO разбить на документы по сериализаторам, как со сваггером
            // TODO урл протестить в servers
            // TODO оверрайднутые урлы пробрасывать в method.servers
            app.UseMiddleware<OpenApiMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUiWithJsonRpc();
            app.UseMvc();
        }
    }
}
