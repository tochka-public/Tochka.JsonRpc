using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tochka.JsonRpc.Server.Pipeline;

namespace Tochka.JsonRpc.Server.IntegrationTests
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
            /*
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.SlidingExpiration = false;
                })
                ;
            */
            services.AddControllers(options =>
                    options.EnableEndpointRouting = false)
                .AddNewtonsoftJson()
                .AddJsonRpcServer();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                /*
                .UseAuthentication()
                .UseDefaultFiles()
                .UseStaticFiles()
                */
                .UseMiddleware<JsonRpcMiddleware>()
                .UseMvc();
        }
    }
}