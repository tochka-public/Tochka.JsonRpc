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
using Tochka.JsonRpc.Swagger;
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
                .AddJsonRpcServer(options =>
                {
                    options.DetailedResponseExceptions = true;
                    options.AllowRawResponses = true;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.TryAddJsonRpcSerializer<CamelCaseJsonRpcSerializer>();
            services.AddSwaggerWithJsonRpc();
            services.AddOpenRpc();
            services.AddDefaultOpenRpcDocument();
        }
        

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
